using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinPDF.Util
{
    public static class SetDotNet
    {
        public static void Run(string requiredVersion = "8.0.0")
        {
            if (!IsDotNetRuntimeInstalled(requiredVersion))
            {
                Debug.WriteLine($".NET {requiredVersion} Runtime is not installed. Downloading and installing...");
                DownloadAndInstallDotNetRuntime(requiredVersion);
            }
            else
            {
                Debug.WriteLine($".NET {requiredVersion} Runtime is already installed.");
            }
        }

        private static bool IsDotNetRuntimeInstalled(string version)
        {
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains($"Microsoft.NETCore.App {version}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking .NET runtime: {ex.Message}");
                return false;
            }
        }

        private static void DownloadAndInstallDotNetRuntime(string version)
        {
            string url = GetDownloadUrlForRuntime(version);
            string fileName = $"dotnet-runtime-{version}-win-x64.exe";

            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    response.Content.CopyToAsync(fs).Wait();
                }
            }

            Process installProcess = new Process();
            installProcess.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = "/quiet /norestart",
                UseShellExecute = false
            };
            installProcess.Start();
            installProcess.WaitForExit();

            Debug.WriteLine(".NET Runtime installation completed.");
        }

        private static string GetDownloadUrlForRuntime(string version)
        {
            // Check if the OS is Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"https://dotnetcli.azureedge.net/dotnet/Runtime/{version}/dotnet-runtime-{version}-win-x64.exe";
            }
            else
            {
                throw new PlatformNotSupportedException("This script only supports downloading .NET runtime on Windows.");
            }
        }
    }
}
