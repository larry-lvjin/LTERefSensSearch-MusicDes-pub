using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LteRefSensTester.Helpers
{
    /// <summary>
    /// P/Invoke SetupAPI 枚举当前 present 的 USB CDC 串口。
    ///
    /// 语义对齐 pyserial `serial/tools/list_ports_windows.py::iterate_comports`：
    ///   SetupDiClassGuidsFromNameW("Ports", ...)                → Ports 设备类 GUID
    ///   SetupDiGetClassDevsW(&guid, null, IntPtr.Zero, DIGCF_PRESENT)  → 只列 present 设备
    ///   SetupDiEnumDeviceInfo(loop index)
    ///   SetupDiGetDeviceRegistryProperty(SPDRP_HARDWAREID)      → hwid 列表（"USB\\VID_.." 才纳入）
    ///   SetupDiGetDeviceRegistryProperty(SPDRP_FRIENDLYNAME)    → "USB Serial Device (COM10)"
    ///
    /// 与旧版读注册表 HKLM\HARDWARE\DEVICEMAP\SERIALCOMM 过滤设备名含 "USB" 相比：
    ///   ① 不受 stale 注册表条目污染（拔了的设备不会返回）
    ///   ② hwid 判断是 USB\ 前缀（USB VID/PID）而非驱动写的 Device Name
    ///   ③ 覆盖驱动名为 \Device\Serial* 的 USB CDC（旧逻辑会漏）
    ///
    /// 该实现是参考 sim_simulator_tool.exe 反编译得到的 pyserial 使用方式。
    /// </summary>
    public static class SetupApiPortEnumerator
    {
        // SetupAPI 常量
        private const uint DIGCF_PRESENT       = 0x00000002;
        private const uint SPDRP_HARDWAREID    = 0x00000001;
        private const uint SPDRP_FRIENDLYNAME  = 0x0000000C;
        private const int  ERROR_NO_MORE_ITEMS      = 259;
        private const int  ERROR_INSUFFICIENT_BUFFER = 122;

        // FriendlyName 里抓 "(COMxx)" 尾巴
        private static readonly Regex ComPortRegex = new(@"\((COM\d+)\)", RegexOptions.Compiled);

        /// <summary>
        /// 枚举所有 hwid 以 "USB\" 开头的当前 present COM 口。
        /// 返回 (Port, Hwid, Friendly)：
        ///   Port     - "COM10"
        ///   Hwid     - "USB\VID_0525&PID_A4A7&MI_00"（多值取第一个）
        ///   Friendly - "USB Serial Device (COM10)"
        /// </summary>
        public static IEnumerable<(string Port, string Hwid, string Friendly)> EnumerateUsbCdcPorts()
        {
            var results = new List<(string, string, string)>();

            // ① 拿到 "Ports" 设备类 GUID
            Guid portsGuid = GetPortsClassGuid();
            if (portsGuid == Guid.Empty) return results;

            // ② 打开 present 设备集合
            IntPtr devInfoSet = SetupDiGetClassDevsW(ref portsGuid, null, IntPtr.Zero, DIGCF_PRESENT);
            if (devInfoSet == IntPtr.Zero || devInfoSet == new IntPtr(-1))
                return results;

            try
            {
                var devInfo = new SP_DEVINFO_DATA();
                devInfo.cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>();

                for (uint index = 0; ; index++)
                {
                    if (!SetupDiEnumDeviceInfo(devInfoSet, index, ref devInfo))
                    {
                        int err = Marshal.GetLastWin32Error();
                        if (err == ERROR_NO_MORE_ITEMS) break;
                        // 其他错误也停 —— 不阻塞主流程
                        break;
                    }

                    // ③ 读 HARDWAREID（多字符串数组）
                    string[] hwids = GetRegistryPropertyMultiSz(devInfoSet, ref devInfo, SPDRP_HARDWAREID);
                    string? usbHwid = null;
                    foreach (var h in hwids)
                    {
                        if (h.StartsWith("USB\\", StringComparison.OrdinalIgnoreCase))
                        {
                            usbHwid = h;
                            break;
                        }
                    }
                    if (usbHwid == null) continue;

                    // ④ 读 FRIENDLYNAME，抽 "(COMxx)"
                    string friendly = GetRegistryPropertySz(devInfoSet, ref devInfo, SPDRP_FRIENDLYNAME);
                    var m = ComPortRegex.Match(friendly);
                    if (!m.Success) continue;

                    results.Add((m.Groups[1].Value, usbHwid, friendly));
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(devInfoSet);
            }

            return results;
        }

        // ── 内部：GUID / 属性读取 ────────────────────────────────────────

        private static Guid GetPortsClassGuid()
        {
            // SetupDiClassGuidsFromNameW 用法：第一次 NULL/0 拿到 requiredSize，第二次分配数组
            uint required = 0;
            SetupDiClassGuidsFromNameW("Ports", null, 0, ref required);
            if (required == 0) return Guid.Empty;

            var guids = new Guid[required];
            if (!SetupDiClassGuidsFromNameW("Ports", guids, required, ref required))
                return Guid.Empty;
            return guids[0];
        }

        private static string[] GetRegistryPropertyMultiSz(IntPtr devInfoSet, ref SP_DEVINFO_DATA devInfo, uint prop)
        {
            byte[] buf = GetRegistryPropertyBytes(devInfoSet, ref devInfo, prop);
            if (buf.Length == 0) return Array.Empty<string>();
            // Unicode multi-sz: 逐 '\0' 分割，末尾双 '\0' 终止
            string all = Encoding.Unicode.GetString(buf);
            var list = new List<string>();
            foreach (var s in all.Split('\0'))
            {
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
            return list.ToArray();
        }

        private static string GetRegistryPropertySz(IntPtr devInfoSet, ref SP_DEVINFO_DATA devInfo, uint prop)
        {
            byte[] buf = GetRegistryPropertyBytes(devInfoSet, ref devInfo, prop);
            if (buf.Length == 0) return "";
            string s = Encoding.Unicode.GetString(buf);
            int nul = s.IndexOf('\0');
            return nul >= 0 ? s.Substring(0, nul) : s;
        }

        private static byte[] GetRegistryPropertyBytes(IntPtr devInfoSet, ref SP_DEVINFO_DATA devInfo, uint prop)
        {
            uint required = 0;
            SetupDiGetDeviceRegistryPropertyW(devInfoSet, ref devInfo, prop,
                out _, null, 0, ref required);
            int err = Marshal.GetLastWin32Error();
            if (required == 0) return Array.Empty<byte>();
            if (err != ERROR_INSUFFICIENT_BUFFER && err != 0) return Array.Empty<byte>();

            var buf = new byte[required];
            if (!SetupDiGetDeviceRegistryPropertyW(devInfoSet, ref devInfo, prop,
                out _, buf, required, ref required))
                return Array.Empty<byte>();
            return buf;
        }

        // ── P/Invoke ────────────────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiClassGuidsFromNameW(
            string className, [Out] Guid[]? classGuidArray, uint classGuidArraySize, ref uint requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevsW(
            ref Guid classGuid, string? enumerator, IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr devInfoSet, uint memberIndex, ref SP_DEVINFO_DATA devInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryPropertyW(
            IntPtr devInfoSet, ref SP_DEVINFO_DATA devInfoData, uint property,
            out uint propertyRegDataType, byte[]? propertyBuffer, uint propertyBufferSize, ref uint requiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr devInfoSet);
    }
}
