using Openus.AppPath;

namespace Openus.WinPDFv2.Properties
{
    internal static class Program
    {
        private static void Setup()
        {
            if (Directory.Exists(AppData.Root) == false)
            {
                Directory.CreateDirectory(AppData.Root);
            }

            if (Directory.Exists(AppData.Data) == false)
            {
                Directory.CreateDirectory(AppData.Data);
            }
            else
            {
                foreach (var file in Directory.GetFiles(AppData.Data))
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