# LTE RefSens Search — Music Desense（LTESenSearchREMusic）

LTE 参考灵敏度（REFSENS）自动探底工具，基于 **R&S CMW500** 信令测试仪，用于量化 **UE 播放音乐时对接收灵敏度的劣化（音乐互调 / 底噪抬升）**。

程序对每个频段的每个信道执行两趟测量——**音乐前（静默）** 与 **音乐中（UE 播放音乐）**——并在结果表格中给出两者的参考灵敏度（EPRE）、UE 发射功率、UE 上报 RSRP 以及差值，从而直观定位受音乐影响的频段与信道。

> 平台：Windows 桌面（WPF / .NET 8）。被测终端（DUT / UE）通过 USB CDC 串口接受控制（播放音乐、复位、SIM 重连）。

---

## 目录

- [功能特性](#功能特性)
- [系统要求](#系统要求)
- [快速开始](#快速开始)
- [使用流程](#使用流程)
- [结果输出](#结果输出)
- [探底算法](#探底算法)
- [项目结构](#项目结构)
- [架构与关键约定](#架构与关键约定)
- [参考文档](#参考文档)

---

## 功能特性

- **自动 REFSENS 探底**：四阶段步进搜索（±1 → ±0.5 → ±0.1 dB），以 5% BLER 为收敛判据，最终用 3000 子帧确认。
- **音乐前 / 音乐中 A/B 对比**：同一频段组内先静默测一遍，再让 UE 播放音乐测第二遍，输出灵敏度差值（音乐差值）。
- **多频段 / 多信道批量测试**：内置 FDD / TDD 频段表，支持 Low / Mid / High 三信道，或仅测 Mid 信道快跑。
- **同制式 / 跨制式智能信道切换**：FDD↔FDD、TDD↔TDD 仅改参数不断链；FDD↔TDD 自动断链、切换 Subframe Offset、重启小区。
- **断连自愈**：RRC 空闲 / 掉线时分级恢复（等待 → 小区 OFF/ON → 通过串口执行 SIM 复位 `AT+CFUN=1`），连续失败达阈值自动终止防跑飞。
- **附加测量**：UE 发射功率（UE TX Power，可选）与 UE 上报 RSRP（index + RANGe 中点 dBm）。
- **线损校正**：可导入频率-损耗表并上传至 CMW500 校正表（RF1C RX/TX 激活）。
- **初始电平模板**：CSV 导入各频段/信道的起测电平。
- **结果导出**：实时表格 + 一键导出 18 列 Excel，附 SCPI 日志与运行日志。
- **深色 UI + 崩溃兜底**：三层未捕获异常兜底（UI Dispatcher / AppDomain / Task），异常写入 `%LOCALAPPDATA%\LteRefSensTester\crash.log`，长时间断连也不闪退。

---

## 系统要求

| 项 | 要求 |
|---|---|
| 操作系统 | Windows 10/11 x64 |
| 运行时 | .NET 8 Desktop Runtime（`net8.0-windows`）|
| 构建工具 | .NET 8 SDK / Visual Studio 2022+ |
| 测试仪 | R&S CMW500，SCPI over TCP（端口 `5025`）|
| DUT | 支持 USB CDC 串口，可接受 `pa play` / `reset` / AT 指令 |
| 依赖包 | ClosedXML、System.IO.Ports、Microsoft.Extensions.DependencyInjection |

---

## 快速开始

源代码位于 `src/` 目录。

```bash
cd src

# 还原并构建
dotnet restore
dotnet build LTERefSensSearch-MusicDes.csproj -c Release

# 运行
dotnet run --project LTERefSensSearch-MusicDes.csproj
# 或直接运行产物：
#   bin\Release\net8.0-windows\LteRefSensTester.exe
```

> 解决方案：`LTERefSensSearch-MusicDes.sln`　·　程序集：`LteRefSensTester`　·　平台：`x64`

**发布单文件**（可选）：使用 `Properties/PublishProfiles/FolderProfile.pubxml`，或

```bash
dotnet publish LTERefSensSearch-MusicDes.csproj -c Release -r win-x64
```

---

## 使用流程

1. **连接仪表**：在左侧输入 CMW500 IP（默认 `192.0.2.10`）→ 点击 **Connect**。日志出现 `Connected → <*IDN?>` 及各条 `Global init:` 即成功。
2. **（可选）初始电平**：`📤 导出模板` → 编辑 CSV 填入各信道起测电平 → `📥 导入电平`。
3. **（可选）线损表**：勾选 **CMW500 线损表** → `📤 导出模板` → 填写频率-损耗 → `📥 导入线损`（上传至 CMW500 校正表并激活 RF1C RX/TX）。
4. **（可选）UE 发射功率**：勾选 **功率测试 (UE TX Power)**（每点约多耗 4 秒）。
5. **选择 UE 串口**：下拉选择 `自动` 或指定 `COMx`（用于发送 `pa play` / `reset` / SIM `AT+CFUN=1`）。
6. **选择频段**：`All` / `None` 或逐个勾选 FDD / TDD 频段。
7. **开始测试**：`▶ Start (All Channels)`（全信道）或 `▶ Mid Channel Only`（仅中信道快跑）。测试中可 `⏸ Pause` / `▶ Resume` / `■ Stop`。
8. **查看 / 导出**：右侧表格实时刷新；测试完成自动落盘，也可手动 `📄 Export Excel` / `📋 导出 LOG`。

---

## 结果输出

运行产物自动保存到 `D:\SenSearchReport\Run_YYYYMMDD_HHmmss\`：

| 文件 | 内容 |
|---|---|
| `RefSens.xlsx` | 结果表格（18 列，见下） |
| `SCPI.log` | 所有 SCPI 收发记录 |
| `RunLog.log` | 应用运行日志 |

**Excel 18 列**：

```
1  Band            2  Mode              3  Channel           4  EARFCN
5  DL RB           6  UL RB             7  EPRE_音乐前       8  EPRE_音乐中
9  UEPwr_音乐前     10 UEPwr_音乐中
11 RSRP_音乐前(dBm) 12 RSRP_音乐中(dBm)
13 RSRP_音乐前_Idx  14 RSRP_音乐中_Idx
15 音乐差值(dB)     16 Probes           17 Result            18 Note
```

- **音乐差值** = `EPRE_音乐中 − EPRE_音乐前`。
- **Result**：差值落在 `[−5, +5] dB` 判 **PASS**，否则 **FAIL**；无法测得记 **ERROR / SKIP**。
- **RSRP** 同时给出 UE 上报的原始 3GPP index 与 RANGe 区间中点 dBm（如 `-86.5 (54)`）。

---

## 探底算法

在 `TestService.SearchRefSensAsync` 中实现，四阶段步进（BLER 探底以 5% 为锚点）：

```
Init    设起测电平 → 1000 帧 BLER
Step 1  粗探 ±1 dB（≤40 次）：BLER<2% 下探，>20% 上抬，2%~20% 命中带内
Step 2  细探 ±0.5 dB（≤40 次，仅当 Step 1 出带）
Step 3  锁定 ±0.1 dB（≤50 次）：以 5% BLER 边界为收敛点
Step 4  确认  3000 帧验证（≤30 次）：BLER<5% → 输出 REFSENS，否则 +0.1 重试
```

每频段组的测试节拍（`RunRefSensTestAsync`）：

```
for 每个频段组 (Low/Mid/High 共用一次音乐会话)：
    阶段 1  音乐前（静默）：切信道 → 置 -85 → 探底 → (可选)UE TX 功率 → RSRP
    进入音乐：置 -85 → UE reset → pa play → 预热 10 s
    阶段 2  音乐中：切信道 → 置 -85 → 探底(起点 = 音乐前+1) → (可选)UE TX 功率 → RSRP
    退出音乐：UE reset
    频段收尾：STOP EBL、OCNG OFF、RSEP -85
```

**安全护栏**：单次探底 `level > startLevel + 40` 立即熔断；**连续 3 个信道 SKIP/ERROR 则终止整轮**（防 CMW/UE 全失联跑飞）。

---

## 项目结构

```
src/
├─ App.xaml / App.xaml.cs            WPF 入口 + DI 容器 + 三层崩溃兜底
├─ MainWindow.xaml / .xaml.cs        UI：左参数面板 + 右结果表格 + SCPI 日志
├─ Models/
│  ├─ BandConfig.cs                  频段表行（Band/Mode/EARFCN/RB/起测电平）
│  ├─ ChannelPoint.cs                (Band, Low/Mid/High, EARFCN)
│  ├─ DuplexMode.cs                  FDD | TDD
│  ├─ RefSensRow.cs                  结果表行 VM（含派生显示字符串/颜色）
│  ├─ TestConfig.cs                  测试配置（IP/起测电平/门限/串口等）
│  └─ LossTableEntry.cs              频率-损耗对
├─ Services/
│  ├─ ICmwService / CmwService       CMW500 SCPI TCP 客户端（端口 5025）
│  ├─ ILogService / LogService       日志分发（UI + 文件）
│  ├─ ITestService / TestService     核心：探底 / 信道切换 / 测量 / 断连自愈
│  └─ UeSerialManager               DUT USB-CDC 串口单例（音乐/复位/AT）
├─ ViewModels/
│  ├─ ViewModelBase.cs               PropertyChanged 帮助类
│  └─ MainViewModel.cs               命令 / 频段选择 / 连接 / 导入导出 / 日志
├─ Commands/                         RelayCommand、AsyncRelayCommand
├─ Helpers/                          BuildInfo、SetupApiPortEnumerator（COM 口枚举）
├─ Protocol/                         FrameCodec、SerialClient
├─ LossTable.cs / LossEditorWindow.cs  线损内存表与编辑弹窗
└─ Properties/PublishProfiles/       发布配置
```

非源码资产：`RefSens_Example.csv`、`app_music.ico`，及中文规格表 `探底算法RE播音.xlsx`、`连接操作流程*.xlsx`、`音乐前音乐中对比.xlsx`。

---

## 架构与关键约定

- **MVVM + 依赖注入**：`App.xaml.cs` 用 `Microsoft.Extensions.DependencyInjection` 将 `ICmwService` / `ITestService` / `ILogService` 及 `MainViewModel` 注册为单例。
- **SCPI 通道**：所有仪表指令经 `CmwService`（TCP 5025）；全局初始化在连接时执行（`*IDN?`、`MEV MSUBframes`、`FrameTrigger`、`UEReport ENABle/RINTerval`）。
- **UE 串口单例**：所有对 DUT 的 `pa play` / `reset` / AT 指令必须经 `UeSerialManager`，禁止直接 `new SerialPort`。
- **锁定代码（不可改动）**：`TestService` 中信道切换与 EBL 管理相关方法为 `LOCKED`，其行为不变式（同制式仅改参不断链、跨制式必断链重启小区、`MeasureBler` 每次自跑 EBL STOP→INIT 周期等）在 `CLAUDE.md` / `PROJECT_STATE.md` 中详述，修改前务必阅读。
- **本地化裁剪**：仅保留 `zh-Hans;en` 卫星程序集以减小发布体积。

> 完整的 SCPI 指令清单、数据模型 `RefSensRow` 字段、频段表、断连恢复细节等，见 **`src/PROJECT_STATE.md`**（单一事实源，建议续接工作前先读）。

---

## 参考文档

| 文档 | 说明 |
|---|---|
| `src/PROJECT_STATE.md` | 架构 / 算法 / SCPI 全清单 / 数据模型 / 变更日志（**首要参考**）|
| `src/CONNECTION_TEST_PROCEDURE.md` | 连接测试操作流程（初始连接三情况、同制式/跨制式切换）——只读参考 |
| `src/CLAUDE.md` / `src/AGENTS.md` | 只读文件与 LOCKED 代码约定 |

> 注：`CONNECTION_TEST_PROCEDURE.md` 等标注为「只读参考文档」的文件不得随意编辑、覆盖或删除。
</content>
</invoke>
