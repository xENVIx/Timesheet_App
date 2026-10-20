

namespace Timesheeter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();


            // Up while the database and data load (the Factory constructor) and the main window
            // builds its pages; it fades out once the main window is on screen.
            var splash = Forms.FrmSplash.ShowNow();

            Factory factory = new Factory();
            factory.Run(splash);

        }

        
    }
}