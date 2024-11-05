using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Openus.WinPDFv2.Properties;
using Openus.AppPath;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;

namespace Openus.WinPDFv2
{
    public partial class MainForm : Form
    {
        private TableLayoutPanel _tableLayout;
        private MenuStrip _menuStrip;
        private WebView2 _webView;
        private TableLayoutPanel _rawList;
        private TableLayoutPanel _resultList;
        private ProgressBar _mainProgressBar;

        private int _id = 0;
        private string[] _beginArgs;

        public MainForm(string[] args)
        {
            /// Set Properties
            {
                Text = "WinPDF v2";

                StartPosition = FormStartPosition.CenterScreen;
                ClientSize = GlobalSetting.Default.WindowSize;
                Icon = GlobalResource.OpenusIcon;

                Shown += MainFormShown;
                FormClosed += MainFormClosed;

                _beginArgs = args;
            }

            /// Set Elements
            {
                _tableLayout = new TableLayoutPanel()
                {
                    Parent = this,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    ColumnCount = 2,
                    RowCount = 4,
                };

                _menuStrip = new MenuStrip()
                {
                    Parent = _tableLayout,
                };

                CoreWebView2Environment webView2Environment
                    = CoreWebView2Environment.CreateAsync(null, AppData.Root).Result;

                _webView = new WebView2()
                {
                    Parent = _tableLayout,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                };

                _webView.EnsureCoreWebView2Async(webView2Environment);
                _webView.Source = new Uri(GlobalResource.HomePage);

                GroupBox raw = new GroupBox()
                {
                    Parent = _tableLayout,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Text = "Raw",
                };

                _rawList = new TableLayoutPanel()
                {
                    Parent = raw,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    AutoScroll = true,
                    RowCount = 30,
                    ColumnCount = 1,
                };

                GroupBox result = new GroupBox()
                {
                    Parent = _tableLayout,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Text = "Result",
                };

                _resultList = new TableLayoutPanel()
                {
                    Parent = result,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    AutoScroll = true,
                    RowCount = 30,
                    ColumnCount = 1,
                };

                _mainProgressBar = new ProgressBar()
                {
                    Parent = _tableLayout,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Value = 0,
                };

                ToolStripMenuItem open = (ToolStripMenuItem)_menuStrip.Items.Add("Open");
                ToolStripMenuItem save = (ToolStripMenuItem)_menuStrip.Items.Add("Save");
                ToolStripMenuItem tool = (ToolStripMenuItem)_menuStrip.Items.Add("Tool");
                ToolStripMenuItem info = (ToolStripMenuItem)_menuStrip.Items.Add("Info");

                open.DropDownItems.Add("Open PDF").Click += OpenPdfClick;
                save.DropDownItems.Add("Save Merged Result PDF").Click += SaveMergedPdfClick;
                save.DropDownItems.Add("Save Split Result PDF").Click += SaveSplitPdfClick;
                tool.DropDownItems.Add("Remove All Raw PDF").Click += ToolRemoveAllRawClick;
                tool.DropDownItems.Add("Remove All Result PDF").Click += ToolRemoveAllResultClick;
                tool.DropDownItems.Add("Append All Raw to Result PDF").Click += ToolAppendAllClick;
                tool.DropDownItems.Add("Preview Result PDF").Click += ToolPreviewlClick;
                info.DropDownItems.Add("Dependencies").Click += InfoDependencyClick;

                _tableLayout.RowStyles.Add(new RowStyle(height: _menuStrip.Height, sizeType: SizeType.Absolute));
                _tableLayout.RowStyles.Add(new RowStyle(height: 1, sizeType: SizeType.Percent));
                _tableLayout.RowStyles.Add(new RowStyle(height: 1, sizeType: SizeType.Percent));
                _tableLayout.RowStyles.Add(new RowStyle(height: 15, sizeType: SizeType.Absolute));

                _tableLayout.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));
                _tableLayout.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));

                _tableLayout.SetRow(_menuStrip, 0);
                _tableLayout.SetColumnSpan(_menuStrip, 2);
                _tableLayout.SetRow(_webView, 1);
                _tableLayout.SetRowSpan(_webView, 2);
                _tableLayout.SetColumn(_webView, 0);
                _tableLayout.SetRow(raw, 1);
                _tableLayout.SetColumn(raw, 1);
                _tableLayout.SetRow(result, 2);
                _tableLayout.SetColumn(result, 1);
                _tableLayout.SetRow(_mainProgressBar, 3);
                _tableLayout.SetColumnSpan(_mainProgressBar, 2);
            }
        }

        private void MainFormShown(object? sender, EventArgs e)
        {
            if (_beginArgs.Length == 0)
            {
                return;
            }

            List<string> files = new List<string>();

            OpenPdfFromSetup(_beginArgs, files);
            LoadPdf(files.ToArray());
        }

        private void MainFormClosed(object? sender, FormClosedEventArgs e)
        {
            GlobalSetting.Default.WindowSize = ClientSize;
            GlobalSetting.Default.Save();
        }

        private void UseProgressBar(int mainValue, int mainMax)
        {
            _mainProgressBar.Minimum = 0;
            _mainProgressBar.Maximum = mainMax;
            _mainProgressBar.Value = mainValue;
        }

        private void EndProgressBar()
        {
            new Thread(() =>
            {
                Invoke(async () =>
                {
                    UseProgressBar(1, 1);
                    await Task.Delay(500);
                    UseProgressBar(0, 1);
                });
            }).Start();
        }

        private void OpenPdfFromSetup(string[] args, List<string> files)
        {
            foreach (string arg in args)
            {
                if (Directory.Exists(arg) == true)
                {
                    DirectoryInfo info = new DirectoryInfo(arg);
                    List<string> subdirs = info.GetDirectories().ToList().ConvertAll(x => x.FullName);
                    List<string> subfiles = info.GetFiles().ToList().ConvertAll(x => x.FullName);

                    OpenPdfFromSetup(subdirs.Concat(subfiles).ToArray(), files);
                }
                else if (File.Exists(arg) == true)
                {
                    files.Add(arg);
                }
            }
        }

        private void LoadPdf(string[] files)
        {
            int seqence = 0;
            object locker = new object();
            Thread[] tasks = new Thread[files.Length];
            List<string> failedList = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                int x = i;

                tasks[x] = new Thread(() =>
                {
                    string file = files[x];

                    try
                    {
                        PdfDocument pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                        while (true)
                        {
                            lock (locker)
                            {
                                if (x == seqence)
                                {
                                    Invoke(() =>
                                    {
                                        RawPdfControl control
                                            = new RawPdfControl(pdf, _id++, _webView, _resultList)
                                            {
                                                Parent = _rawList,
                                                Dock = DockStyle.Fill,
                                                Height = 30,
                                            };
                                        _rawList.SetRow(control, x);

                                        RawPdfControl.Pdfs.Add(control);

                                        UseProgressBar(seqence, files.Length);
                                    });

                                    seqence++;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        while (true)
                        {
                            lock (locker)
                            {
                                if (x == seqence)
                                {
                                    Invoke(() =>
                                    {
                                        failedList.Add(file);

                                        UseProgressBar(seqence, files.Length);
                                    });

                                    seqence++;
                                    break;
                                }
                            }
                        }
                    }
                });

                tasks[x].Start();
            }

            new Thread(() =>
            {
                while (true)
                {
                    lock (locker)
                    {
                        if (seqence == files.Length)
                        {
                            EndProgressBar();

                            if (failedList.Count > 0)
                            {
                                MessageBox.Show("Fail to open some PDFs\n\n" + string.Join("\n", failedList), GlobalResource.SystemText);
                            }

                            break;
                        }
                    }
                }
            }).Start();
        }
    }
}
