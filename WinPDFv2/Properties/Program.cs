using Openus.AppPath;
using System.Globalization;

namespace Openus.WinPDFv2.Properties
{
    internal static class Program
    {
        private static void Setup()
        {
            if (Directory.Exists(AppData.Root) == false)
            {
                Directory.CreateDirectory(AppData.Root);

                using (StreamWriter sw = new StreamWriter(AppData.Lang))
                {
                    sw.Write(CultureInfo.CurrentUICulture.Name);
                }
            }
            else
            {
                if (File.Exists(AppData.Lang) == true)
                {
                    using (StreamReader sr = new StreamReader(AppData.Lang))
                    {
                        string lang = sr.ReadToEnd();

                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter(AppData.Lang))
                    {
                        sw.Write(CultureInfo.CurrentUICulture.Name);
                        
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
                    }
                }
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
        static void Main(string[] args)
        {
            Setup();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(args));
        }
    }
}