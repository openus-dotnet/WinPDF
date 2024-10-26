using Microsoft.Web.WebView2.WinForms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace WinPDF2
{
    public class MainForm : Form
    {
        private TableLayoutPanel _fullContainer;
        private MenuStrip _menuStrip;
        private WebView2 _webView;
        private TableLayoutPanel _rawList;
        private TableLayoutPanel _resultList;

        private int _id = 0;

        public MainForm()
        {
            if (Directory.Exists(Program.AppFolder) == false)
            {
                Directory.CreateDirectory(Program.AppFolder);
            }

            foreach (var file in Directory.GetFiles(Program.AppFolder))
            {
                File.Delete(file);
            }

            /// Set Properties
            {
                Text = "WinPDF v2";
                ClientSize = new Size(800, 600);
            }

            /// Set Elements
            {
                _fullContainer = new TableLayoutPanel()
                {
                    Parent = this,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    ColumnCount = 2,
                    RowCount = 3,
                };

                _menuStrip = new MenuStrip()
                {
                    Parent = _fullContainer,
                };

                _webView = new WebView2()
                {
                    Parent = _fullContainer,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                };

                GroupBox raw = new GroupBox()
                {
                    Parent = _fullContainer,
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
                    Parent = _fullContainer,
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

                _fullContainer.RowStyles.Add(new RowStyle(height: _menuStrip.Height, sizeType: SizeType.Absolute));
                _fullContainer.RowStyles.Add(new RowStyle(height: 1, sizeType: SizeType.Percent));
                _fullContainer.RowStyles.Add(new RowStyle(height: 1, sizeType: SizeType.Percent));

                _fullContainer.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));
                _fullContainer.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));

                _fullContainer.SetRow(_menuStrip, 0);
                _fullContainer.SetColumnSpan(_menuStrip, 2);
                _fullContainer.SetRow(_webView, 1);
                _fullContainer.SetRowSpan(_webView, 2);
                _fullContainer.SetColumn(_webView, 0);
                _fullContainer.SetRow(raw, 1);
                _fullContainer.SetColumn(raw, 1);
                _fullContainer.SetRow(result, 2);
                _fullContainer.SetColumn(result, 1);
            }
        }

        private void ToolPreviewlClick(object? sender, EventArgs e)
        {
            PdfDocument pdfDocument = new PdfDocument();

            foreach (var document in ResultPdfControl.Pdfs)
            {
                foreach (var page in document.Document.Pages)
                {
                    if (page != null)
                    {
                        pdfDocument.AddPage(page);
                    }
                }
            }

            string temp = Path.Combine(Program.AppFolder, "temp-merge.pdf");

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
            foreach (var control in RawPdfControl.Pdfs)
            {
                control.AddButtonClick(control, new EventArgs());
            }
        }

        private void SaveMergedPdfClick(object? sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "PDF|*.pdf",
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PdfDocument pdfDocument = new PdfDocument();
                
                foreach (var document in ResultPdfControl.Pdfs)
                {
                    foreach (var page in document.Document.Pages)
                    {
                        if (page != null)
                        {
                            pdfDocument.AddPage(page);
                        }
                    }
                }

                pdfDocument.Save(dialog.FileName);
            }
        }

        private void SaveSplitedPdfClick(object? sender, EventArgs e)
        {
            NumberInputForm form = new NumberInputForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "PDF|*.pdf",
                };

                int range = form.Submit;
                int i = 1;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PdfDocument pdfDocument = new PdfDocument();
    
                    foreach (var document in ResultPdfControl.Pdfs)
                    {
                        foreach (var page in document.Document.Pages)
                        {
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

                                i++;
                            }
                        }
                    }

                    if (range != 1)
                    {
                        pdfDocument.Save(dialog.FileName.Insert(dialog.FileName.Length - 4,
                                            $" [{(i - (i % range) + 1).ToString().PadLeft(4, '0')}-{(i - 1).ToString().PadLeft(4, '0')}]"));
                        pdfDocument = new PdfDocument();
                    }
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

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    int x = i;

                    new Thread(() =>
                    {
                        string file = dialog.FileNames[x];
                        PdfDocument pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                        while (true)
                        {
                            lock (locker) if (x == id) break;
                        }

                        Invoke(() =>
                        {
                            RawPdfControl control = new RawPdfControl(pdf, _id++, _webView, _resultList)
                            {
                                Parent = _rawList,
                                Dock = DockStyle.Fill,
                                Height = 30,
                            };

                            _rawList.SetRow(control, x);
                            RawPdfControl.Pdfs.Add(control);
                        });

                        lock(locker) id++;
                    }).Start();
                }
            }
        }
    }
}
