using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LteRefSensTester.Helpers
{
    internal static class BuildInfo
    {
        public static DateTime BuildTimeUtc { get; }
        public static TimeSpan ValidFor { get; } = TimeSpan.FromMinutes(518400);

        static BuildInfo()
        {
            DateTime t = DateTime.UtcNow;
            try
            {
                var attr = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "BuildTime");
                if (attr != null &&
                    DateTime.TryParse(attr.Value, CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var parsed))
                {
                    t = parsed.ToUniversalTime();
                }
            }
            catch { }
            BuildTimeUtc = t;
        }

        public static bool IsExpired => (DateTime.UtcNow - BuildTimeUtc) > ValidFor;
    }
}
