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
        private static string InstallPath = @"C:\Program Files\Openus.NET\winpdf";
        private static string MsiPath = @"C:\Program Files\Openus\installer";
        private static string GitHubApiUrl = @"https://api.github.com/repos/openus-dotnet/winpdf/releases/latest";
        private static string DesktopShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\WinPDF.lnk";
        private static string ExecutableName = "Openus.WinPDF2.exe";
        private static string MsiFileName = "Openus.WinPDF2.Installer.exe";

        public static async Task Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0].ToLower() == "uninstall")
                {
                    UninstallApp();
                    return;
                }

                // Download latest release
                string downloadedFile = await DownloadLatestRelease();
                Console.WriteLine($"Downloaded: {downloadedFile}");

                // Install the app and copy the MSI file
                InstallApp(downloadedFile);

                // Create desktop shortcut
                CreateShortcut(DesktopShortcutPath, Path.Combine(InstallPath, ExecutableName));

                // Register app with MSI uninstall command
                RegisterApp("WinPDFv2");

                Console.WriteLine("Installation completed successfully!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadLine();
            }
        }

        private static async Task<string> DownloadLatestRelease()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# Installer");

                var response = await client.GetStringAsync(GitHubApiUrl);
                var json = JObject.Parse(response);
                var downloadUrl = json["assets"]![0]!["browser_download_url"]!.ToString();

                string tempPath = Path.GetTempPath() + "latest_release.zip";
                var downloadData = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempPath, downloadData);

                return tempPath;
            }
        }

        private static void InstallApp(string zipPath)
        {
            if (!Directory.Exists(InstallPath))
            {
                Directory.CreateDirectory(InstallPath);
            }
            if (!Directory.Exists(MsiPath))
            {
                Directory.CreateDirectory(MsiPath);
            }

            // Extract ZIP contents to installation path
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, InstallPath, true);

            // Copy MSI file to installation path
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory);

            foreach (FileInfo info in dir.GetFiles())
            {
                File.Copy(info.FullName, Path.Combine(MsiPath, info.Name), true);
            }

            Console.WriteLine($"App installed to: {InstallPath}");
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
        private static void RegisterApp(string appName)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{appName}";

            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(registryPath))
            {
                key.SetValue("DisplayName", appName);
                key.SetValue("DisplayIcon", Path.Combine(InstallPath, ExecutableName));
                key.SetValue("InstallLocation", InstallPath);

                // Register the MSI uninstall command
                string uninstallCommand = $"\"{MsiPath}\\{MsiFileName}\" uninstall";
                key.SetValue("UninstallString", uninstallCommand);

                key.SetValue("Publisher", "Openus.NET");
            }

            Console.WriteLine("App registered in the registry with MSI uninstall command.");
        }

        [SupportedOSPlatform("windows")]
        private static void UninstallApp()
        {
            try
            {
                // Remove the desktop shortcut
                if (File.Exists(DesktopShortcutPath))
                {
                    File.Delete(DesktopShortcutPath);
                    Console.WriteLine("Desktop shortcut removed.");
                }

                // Delete the installation directory
                if (Directory.Exists(InstallPath))
                {
                    Directory.Delete(InstallPath, true);
                    Console.WriteLine($"Deleted: {InstallPath}");
                }

                // Remove the registry entry
                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinPDFv2";
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(registryPath, false);
                Console.WriteLine("App unregistered from the registry.");

                Console.WriteLine("Uninstallation completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Uninstallation error: {ex.Message}");
            }
        }
    }
}
