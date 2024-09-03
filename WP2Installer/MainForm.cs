using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WP2Installer
{
    public class MainForm : Form
    {
        private Label ProgressLabel { get; set; }
        private ProgressBar ProgressBar { get; set; }
        private Button OpenDetail { get; set; }
        private RichTextBox ProgressTextBox { get; set; }

        public MainForm()
        {
            Designer();

            new Thread(() =>
            {
                string requiredVersion = "8.0.0";
                if (!IsDotNetRuntimeInstalled(requiredVersion))
                {
                    TaskWriteLine($".NET {requiredVersion} Runtime is not installed. Downloading and installing...");
                    DownloadAndInstallDotNetRuntime(requiredVersion);
                }
                else
                {
                    TaskWriteLine($".NET {requiredVersion} Runtime is already installed.");
                }
            }).Start();
        }

        private void TaskWriteLine(string message)
        {
            Invoke(new Action(() =>
            {
                ProgressLabel.Text = message;
                ProgressTextBox.Text += message + "\n";
            }));
        }

        private void Designer()
        {
            TableLayoutPanel panel = new TableLayoutPanel()
            {
                Visible = true,
                Parent = this,
                Dock = DockStyle.Fill,
            };

            panel.RowStyles.Add(new RowStyle() { Height = 30, SizeType = SizeType.Absolute });
            panel.RowStyles.Add(new RowStyle() { Height = 30, SizeType = SizeType.Absolute });
            panel.RowStyles.Add(new RowStyle() { Height = 30, SizeType = SizeType.Absolute });
            panel.RowStyles.Add(new RowStyle() { Height = 1, SizeType = SizeType.Percent });

            ProgressLabel = new Label()
            {
                Visible = true,
                Dock = DockStyle.Fill,
            };

            panel.Controls.Add(ProgressLabel, 0, 0);

            ProgressBar = new ProgressBar()
            {
                Visible = true,
                Dock = DockStyle.Fill,
            };

            panel.Controls.Add(ProgressBar, 0, 1);
        }

        private bool IsDotNetRuntimeInstalled(string version)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = "--list-runtimes";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains($"Microsoft.NETCore.App {version}");
            }
            catch (Exception ex)
            {
                TaskWriteLine($"Error checking .NET runtime: {ex.Message}");
                return false;
            }
        }

        private void DownloadAndInstallDotNetRuntime(string version)
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
            installProcess.StartInfo.FileName = fileName;
            installProcess.StartInfo.Arguments = "/quiet /norestart";
            installProcess.StartInfo.UseShellExecute = false;
            installProcess.Start();
            installProcess.WaitForExit();

            TaskWriteLine(".NET Runtime installation completed.");
        }

        private string GetDownloadUrlForRuntime(string version)
        {
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
