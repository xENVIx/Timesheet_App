using System.Reflection;

namespace Timesheeter.Core.Data
{
    /// <summary>Fills in <see cref="IAudited"/> fields when records are written.</summary>
    public static class Audit
    {
        /// <summary>Longest MOP allowed (the column's limit).</summary>
        public const int MaxProcessLength = 200;

        /// <summary>
        /// This program and machine, e.g. "Timesheeter 1.0.0 @ DESKTOP-1234"; written to MOP.
        /// Set it to override (e.g. a tool converting data on someone's behalf).
        /// </summary>
        public static string Process
        {
            get => _process;
            set => _process = Truncate(value);
        }

        private static string _process = Truncate(DefaultProcess());

        /// <summary>
        /// Stamps <paramref name="item"/> (if it's <see cref="IAudited"/>) as changed now by this process;
        /// <paramref name="created"/> also sets the creation time.
        /// </summary>
        public static void Stamp(object item, bool created)
        {
            if (item is not IAudited audited) return;

            var now = DateTime.UtcNow;
            if (created || audited.TOC == default) audited.TOC = now;
            audited.TOM = now;
            audited.MOP = Process;
        }

        private static string DefaultProcess()
        {
            var assembly = Assembly.GetEntryAssembly();
            string name = assembly?.GetName().Name ?? "Timesheeter";
            string? version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly?.GetName().Version?.ToString(3);

            // Without the build's "+commit" suffix.
            int plus = version?.IndexOf('+') ?? -1;
            if (plus >= 0) version = version![..plus];

            return string.IsNullOrEmpty(version)
                ? $"{name} @ {Environment.MachineName}"
                : $"{name} {version} @ {Environment.MachineName}";
        }

        private static string Truncate(string value) =>
            value.Length <= MaxProcessLength ? value : value[..MaxProcessLength];
    }
}
