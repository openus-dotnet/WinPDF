using Microsoft.Web.WebView2.WinForms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Openus.WinPDF2.Properties;

namespace Openus.WinPDF2
{
    public class MainForm : Form
    {
        private TableLayoutPanel _tableLayout;
        private MenuStrip _menuStrip;
        private WebView2 _webView;
        private TableLayoutPanel _rawList;
        private TableLayoutPanel _resultList;
        private ProgressBar _mainProgressBar;

        private int _id = 0;

        public MainForm()
        {
            /// Set Properties
            {
                Text = "WinPDF v2";
                ClientSize = new Size(800, 600);

                using (MemoryStream ms = new MemoryStream(GlobalResource.OpenusIcon))
                {
                    Icon = new Icon(ms);
                }
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

                _webView = new WebView2()
                {
                    Parent = _tableLayout,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Source = new Uri(@"https://sites.google.com/view/openus-dotnet/project/winpdf"),
                };

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

                ToolStripMenuItem file = (ToolStripMenuItem)_menuStrip.Items.Add("File");
                ToolStripMenuItem save = (ToolStripMenuItem)_menuStrip.Items.Add("Save");
                ToolStripMenuItem tool = (ToolStripMenuItem)_menuStrip.Items.Add("Tool");

                file.DropDownItems.Add("Open PDF").Click += FileOpenPdfClick;
                save.DropDownItems.Add("Save Merged Result PDF").Click += SaveMergedPdfClick;
                save.DropDownItems.Add("Save Splited Result PDF").Click += SaveSplitedPdfClick;
                tool.DropDownItems.Add("Remove All Raw PDF").Click += ToolRemoveAllRawClick;
                tool.DropDownItems.Add("Remove All Result PDF").Click += ToolRemoveAllResultClick;
                tool.DropDownItems.Add("Append All Raw to Result PDF").Click += ToolAppendAllClick;
                tool.DropDownItems.Add("Preview Result PDF").Click += ToolPreviewlClick;

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

        private void ToolPreviewlClick(object? sender, EventArgs e)
        {
            if (ResultPdfControl.Pdfs.Count == 0)
            {
                MessageBox.Show("Result PDF count is 0", GlobalResource.SystemText);
                return;
            }

            PdfDocument pdfDocument = new PdfDocument();

            int mainMax = ResultPdfControl.Pdfs.Count;
            int i = 0;

            for (int main = 0; main < ResultPdfControl.Pdfs.Count; main++)
            {
                ResultPdfControl? document = ResultPdfControl.Pdfs[main];

                for (int mini = 0; mini < document.Document.PageCount; mini++)
                {
                    PdfPage page = document.Document.Pages[mini];

                    if (page != null)
                    {
                        Invoke(() => UseProgressBar(main, mainMax));
                        pdfDocument.AddPage(page);

                        i++;
                    }
                }
            }

            string temp = Path.Combine(AppPath.AppDataPath, "temp-merge.pdf");

            Invoke(() => UseProgressBar());
            pdfDocument.Save(temp);

            _webView.Source = new Uri(temp);
        }

        private void ToolRemoveAllResultClick(object? sender, EventArgs e)
        {
            _resultList.Controls.Clear();
            ResultPdfControl.Pdfs.Clear();
        }

        private void ToolRemoveAllRawClick(object? sender, EventArgs e)
        {
            _rawList.Controls.Clear();
            RawPdfControl.Pdfs.Clear();
        }

        private void ToolAppendAllClick(object? sender, EventArgs e)
        {
            if (RawPdfControl.Pdfs.Count == 0)
            {
                MessageBox.Show("Raw PDF count is 0", GlobalResource.SystemText);
                return;
            }

            int max = RawPdfControl.Pdfs.Count;

            for (int i = 0; i < RawPdfControl.Pdfs.Count; i++)
            {
                RawPdfControl? control = RawPdfControl.Pdfs[i];

                control.AddButtonClick(control, new EventArgs());
                Invoke(() => UseProgressBar(i, max));
            }

            Invoke(() => UseProgressBar());
        }

        private void SaveMergedPdfClick(object? sender, EventArgs e)
        {
            if (ResultPdfControl.Pdfs.Count == 0)
            {
                MessageBox.Show("Result PDF count is 0", GlobalResource.SystemText);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "PDF|*.pdf",
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PdfDocument pdfDocument = new PdfDocument();

                int mainMax = ResultPdfControl.Pdfs.Count;
                int i = 0;

                for (int main = 0; main < ResultPdfControl.Pdfs.Count; main++)
                {
                    ResultPdfControl? document = ResultPdfControl.Pdfs[main];

                    for (int mini = 0; mini < document.Document.PageCount; mini++)
                    {
                        PdfPage page = document.Document.Pages[mini];

                        if (page != null)
                        {
                            Invoke(() => UseProgressBar(main, mainMax));
                            pdfDocument.AddPage(page);

                            i++;
                        }
                    }
                }

                Invoke(() => UseProgressBar());
                pdfDocument.Save(dialog.FileName);
            }
        }

        private void SaveSplitedPdfClick(object? sender, EventArgs e)
        {
            if (ResultPdfControl.Pdfs.Count == 0)
            {
                MessageBox.Show("Result PDF count is 0", GlobalResource.SystemText);
                return;
            }

            NumberInputForm form = new NumberInputForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "PDF|*.pdf",
                };

                int mainMax = ResultPdfControl.Pdfs.Count;
                int range = form.Submit;
                int i = 1;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PdfDocument pdfDocument = new PdfDocument();

                    for (int main = 0; main < ResultPdfControl.Pdfs.Count; main++)
                    {
                        ResultPdfControl? document = ResultPdfControl.Pdfs[main];

                        for (int mini = 0; mini < document.Document.PageCount; mini++)
                        {
                            PdfPage page = document.Document.Pages[mini];

                            if (page != null)
                            {
                                pdfDocument.AddPage(page);

                                if (i % range == 0)
                                {
                                    if (range == 1)
                                    {
                                        pdfDocument.Save(dialog.FileName.Insert(dialog.FileName.Length - 4, 
                                            $" [{i.ToString().PadLeft(4, '0')}]"));
                                        pdfDocument = new PdfDocument();
                                    }
                                    else
                                    {
                                        pdfDocument.Save(dialog.FileName.Insert(dialog.FileName.Length - 4, 
                                            $" [{(i - range + 1).ToString().PadLeft(4, '0')}-{i.ToString().PadLeft(4, '0')}]"));
                                        pdfDocument = new PdfDocument();
                                    }
                                }

                                Invoke(() => UseProgressBar(main, mainMax));
                                i++;
                            }
                        }
                    }

                    i--;

                    if (range != 1)
                    {
                        pdfDocument.Save(dialog.FileName.Insert(dialog.FileName.Length - 4,
                                            $" [{(i - (i % range) + 1).ToString().PadLeft(4, '0')}-{i.ToString().PadLeft(4, '0')}]"));
                        pdfDocument = new PdfDocument();
                    }

                    Invoke(() => UseProgressBar());
                }
            }
        }

        private void FileOpenPdfClick(object? sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "PDF|*.pdf",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int id = 0;
                object locker = new object();
                Thread[] tasks = new Thread[dialog.FileNames.Length];
                List<string> failedList = new List<string>();

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    int x = i;

                    tasks[x] = new Thread(() =>
                    {
                        string file = dialog.FileNames[x];

                        try
                        {
                            PdfDocument pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                            while (true)
                            {
                                lock (locker)
                                {
                                    if (x == id)
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

                                            UseProgressBar(id, dialog.FileNames.Length); 
                                        });

                                        id++;
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
                                    if (x == id)
                                    {
                                        Invoke(() =>
                                        {
                                            failedList.Add(file);

                                            UseProgressBar(id, dialog.FileNames.Length);
                                        });

                                        id++;
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
                            if (id == dialog.FileNames.Length)
                            {
                                Invoke(() =>
                                {
                                    UseProgressBar();
                                });

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

        private void UseProgressBar(int mainValue = 0, int mainMax = 100)
        {
            _mainProgressBar.Minimum = 0;
            _mainProgressBar.Maximum = mainMax;
            _mainProgressBar.Value = mainValue;
        }
    }
}
