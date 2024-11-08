using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Openus.AppPath;
using System.IO.Compression;

namespace Openus.Installer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0].ToLower() == "uninstall")
                {
                    UninstallApp();

                    Console.WriteLine("Uninstallation completed successfully!");
                    Console.ReadLine();
                }
                else
                {
                    /// Temp
                    /// 
                    ///string? dotnetUrl = await GetLatestDotNet8RuntimeUrl();
                    ///
                    ///if (dotnetUrl != null)
                    ///{
                    ///    await DownloadAndInstallDotNet8Runtime(dotnetUrl);
                    ///}

                    string downloadedFile = await DownloadLatestRelease();

                    InstallApp(downloadedFile);
                    CreateShortcut();
                    RegisterApp();

                    Console.WriteLine("Installation completed successfully!");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        private static async Task<string> DownloadLatestRelease()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# Installer");

                var response = await client.GetStringAsync(ProgramFiles.GitHubApiUrl);
                var json = JObject.Parse(response);
                var downloadUrl = json["assets"]![0]!["browser_download_url"]!.ToString();

                Console.WriteLine($"Get download URL");
                Console.WriteLine("Downloading... Please to wait");

                string tempPath = Path.GetTempPath() + "latest_release.zip";
                var downloadData = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempPath, downloadData);

                Console.WriteLine($"Downloaded latest release");
                return tempPath;
            }
        }

        private static void InstallApp(string zipPath)
        {
            Console.WriteLine("Check directories");

            if (!Directory.Exists(ProgramFiles.ProgramPath))
            {
                Directory.CreateDirectory(ProgramFiles.ProgramPath);
            }
            if (!Directory.Exists(ProgramFiles.InstallerPath))
            {
                Directory.CreateDirectory(ProgramFiles.InstallerPath);
            }

            Console.WriteLine("Extract lastest release");

            ZipFile.ExtractToDirectory(zipPath, ProgramFiles.ProgramPath, true);
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory);

            foreach (FileInfo info in dir.GetFiles())
            {
                File.Copy(info.FullName, Path.Combine(ProgramFiles.InstallerPath, info.Name), true);
            }

            Console.WriteLine($"App installed");
        }

        private static void CreateShortcut()
        {
            string targetPath = Path.Combine(ProgramFiles.ProgramPath, ProgramFiles.ProgramName);

            string psCommand = $@"
                $WScript = New-Object -ComObject WScript.Shell;
                $Shortcut = $WScript.CreateShortcut('{ProgramFiles.DesktopShortcutPath}');
                $Shortcut.TargetPath = '{targetPath}';
                $Shortcut.Save();
            ";

            ProcessStartInfo psi = new ProcessStartInfo("powershell", $"-Command \"{psCommand}\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
            Console.WriteLine("Shortcut created on the desktop");
        }

        [SupportedOSPlatform("windows")]
        private static void RegisterApp()
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{ProgramFiles.ProgramName}";

            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(registryPath))
            {
                key.SetValue("DisplayName", ProgramFiles.ProgramName);
                key.SetValue("DisplayIcon", Path.Combine(ProgramFiles.ProgramPath, ProgramFiles.ProgramName));
                key.SetValue("InstallLocation", ProgramFiles.ProgramPath);

                string uninstallCommand = $"\"{ProgramFiles.InstallerPath}\\{ProgramFiles.InstallerName}\" uninstall";

                key.SetValue("UninstallString", uninstallCommand);
                key.SetValue("Publisher", "Openus.NET");
            }

            Console.WriteLine("App registered in the registry");
        }

        [SupportedOSPlatform("windows")]
        private static void UninstallApp()
        {
            try
            {
                if (File.Exists(ProgramFiles.DesktopShortcutPath))
                {
                    File.Delete(ProgramFiles.DesktopShortcutPath);
                    Console.WriteLine("Desktop shortcut removed");
                }

                if (Directory.Exists(ProgramFiles.ProgramPath))
                {
                    Directory.Delete(ProgramFiles.ProgramPath, true);
                    Console.WriteLine("Program files removed");
                }

                string registryPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{ProgramFiles.ProgramName}";

                Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(registryPath, false);
                Console.WriteLine("App unregistered from the registry");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static async Task DownloadAndInstallDotNet8Runtime(string url)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "dotnet-runtime-8.0.0-win-x64.exe");

            using (HttpClient client = new HttpClient())
            {
                byte[] fileBytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempFilePath, fileBytes);
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        Arguments = "/install /quiet /norestart",
                        UseShellExecute = true,
                        Verb = "runas",
                    }
                };

                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private static async Task<string?> GetLatestDotNet8RuntimeUrl()
        {
            string apiUrl = "https://api.github.com/repos/dotnet/core/releases/latest";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                var response = await client.GetStringAsync(apiUrl);
                var json = JObject.Parse(response);

                foreach (var asset in json["assets"]!)
                {
                    string name = asset["name"]!.ToString();
                    string downloadUrl = asset["browser_download_url"]!.ToString();

                    if (name.Contains("dotnet-runtime-8.0") && name.EndsWith("-win-x64.exe"))
                    {
                        return downloadUrl;
                    }
                }
            }
            return null;
        }
    }
}
