using Openus.AppPath;
using Openus.WinPDFv2.Properties;
using PdfSharp.Pdf;

namespace Openus.WinPDFv2
{
    public partial class MainForm : Form
    {
        private void OpenPdfClick(object? sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "PDF|*.pdf",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadPdf(dialog.FileNames);
            }
        }

        private void InfoDependencyClick(object? sender, EventArgs e)
        {
            MessageBox.Show(GlobalResource.Dependencies, "Dependencies");
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

            string temp = Path.Combine(AppData.Data, "temp-merge.pdf");
            pdfDocument.Save(temp);

            EndProgressBar();

            _webView.Source = new Uri(temp);
        }

        private void ToolRemoveAllResultClick(object? sender, EventArgs e)
        {
            DialogResult result
                = MessageBox.Show("Remove all Result group's PDF?", GlobalResource.SystemText, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                _resultList.Controls.Clear();
                ResultPdfControl.Pdfs.Clear();

                EndProgressBar();
            }
        }

        private void ToolRemoveAllRawClick(object? sender, EventArgs e)
        {
            DialogResult result
                = MessageBox.Show("Remove all Raw group's PDF?", GlobalResource.SystemText, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                _rawList.Controls.Clear();
                RawPdfControl.Pdfs.Clear();

                EndProgressBar();
            }
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

            EndProgressBar();
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

                pdfDocument.Save(dialog.FileName);

                EndProgressBar();
            }
        }

        private void SaveSplitPdfClick(object? sender, EventArgs e)
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

                    EndProgressBar();
                }
            }
        }
    }
}
