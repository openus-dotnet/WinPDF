namespace Openus.WinPDF2.Properties
{
    internal static class Program
    {
        private static void Setup()
        {
            if (Directory.Exists(AppPath.AppMainPath) == false)
            {
                Directory.CreateDirectory(AppPath.AppMainPath);
            }
            if (Directory.Exists(AppPath.AppDataPath) == false)
            {
                Directory.CreateDirectory(AppPath.AppDataPath);
            }
            else
            {
                foreach (var file in Directory.GetFiles(AppPath.AppDataPath))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Setup();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}