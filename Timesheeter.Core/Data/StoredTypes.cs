namespace Timesheeter.Core.Data
{
    /// <summary>The record classes kept in a Timesheeter database.</summary>
    public static class StoredTypes
    {
        /// <summary>The timesheet data, shared by the app and the server.</summary>
        public static readonly Type[] Data =
        [
            typeof(Customers.Customer),
            typeof(ProjectCodes.ProjectCode),
            typeof(TimeEntries.TimeEntry),
        ];

        /// <summary>Everything the desktop app stores: the data plus its settings.</summary>
        public static readonly Type[] App = [.. Data, typeof(AppSettings.Setting)];
    }
}
