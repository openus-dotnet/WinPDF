using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Openus.MSI
{
    public class Program
    {
        private static string InstallPath = @"C:\Program Files\winpdf";
        private static string GitHubApiUrl = @"https://api.github.com/repos/openus-dotnet/winpdf/releases/latest";
        private static string DesktopShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\WinPDF.lnk";

        private static async Task Main(string[] args)
        {
            try
            {
                // Download latest release
                string downloadedFile = await DownloadLatestRelease();
                Console.WriteLine($"Downloaded: {downloadedFile}");

                // Install the app
                InstallApp(downloadedFile);

                // Create desktop shortcut
                CreateShortcut(DesktopShortcutPath, Path.Combine(InstallPath, "Openus.WinPDF2.exe"));

                // Register app in the registry
                RegisterApp("WinPDFv2", Path.Combine(InstallPath, "Openus.WinPDF2.exe"));

                Console.WriteLine("Installation completed successfully!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task<string> DownloadLatestRelease()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# Installer");

                // Get the latest release info from GitHub
                var response = await client.GetStringAsync(GitHubApiUrl);
                var json = JObject.Parse(response);
                var downloadUrl = json["assets"]![0]!["browser_download_url"]!.ToString();

                // Download the release asset
                string tempPath = Path.GetTempPath() + "latest_release.zip";
                var downloadData = await client.GetByteArrayAsync(downloadUrl);
                await System.IO.File.WriteAllBytesAsync(tempPath, downloadData);

                return tempPath;
            }
        }

        private static void InstallApp(string zipPath)
        {
            // Create the install directory if it doesn't exist
            if (!Directory.Exists(InstallPath))
            {
                Directory.CreateDirectory(InstallPath);
            }

            // Extract the ZIP file to the install directory
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, InstallPath, true);

            Console.WriteLine("App installed to: " + InstallPath);
        }

        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            string psCommand = $@"
                $WScript = New-Object -ComObject WScript.Shell;
                $Shortcut = $WScript.CreateShortcut('{shortcutPath}');
                $Shortcut.TargetPath = '{targetPath}';
                $Shortcut.Save();
            ";

            ProcessStartInfo psi = new ProcessStartInfo("powershell", $"-Command \"{psCommand}\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();

            Console.WriteLine("Shortcut created on the desktop.");
        }

        [SupportedOSPlatform("windows")]
        private static void RegisterApp(string appName, string appPath)
        {
            // Register the application in the "Apps & Features" section of the Control Panel
            string registryPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{appName}";
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(registryPath))
            {
                key.SetValue("DisplayName", appName);
                key.SetValue("DisplayIcon", appPath);
                key.SetValue("InstallLocation", InstallPath);
                key.SetValue("Publisher", "Openus.NET");
                key.SetValue("UninstallString", appPath + " /uninstall");
            }

            Console.WriteLine("App registered in the registry.");
        }
    }
}