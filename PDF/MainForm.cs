using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDF
{
    public partial class MainForm : Form
    {
        public class PdfNumbering
        {
            public int ID { get; set; }
            public static int LastNumber { get; set; } = 1;
            public PdfDocument Document { get; set; }

            public PdfNumbering(int id, PdfDocument document)
            {
                ID = id;
                Document = document;
            }
        }
        public enum PromptOperater
        {
            None,
            To,
            Add,
            Times,
        }
        public class Prompt
        {
            public int PdfID { get; set; }
            public int PdfPage { get; set; }
            public PromptOperater Operater { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void SelectPDFButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "PDF File|*.pdf",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in dialog.FileNames)
                {
                    PdfDocument docs = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    PdfNumbering wrap = new PdfNumbering(PdfNumbering.LastNumber++, docs);
                    PDFListBox.Items.Add(wrap);
                }
            }

        }

        private void PDFListBox_Format(object sender, ListControlConvertEventArgs e)
        {
            PdfNumbering? pdf = e.ListItem as PdfNumbering;

            if (pdf != null)
            {
                e.Value = pdf.ID.ToString().PadLeft(3) + " " + pdf.Document!.FullPath;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (PdfNumbering pdf in PDFListBox.Items)
                {
                    pdf.Document!.Close();
                }
            }
            catch { }
        }

        private void PDFListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PdfNumbering? pdf = PDFListBox.SelectedItem as PdfNumbering;

            if (pdf != null)
            {
                InfoLabel.Text = "PDF INFO\r\n" +
                    "PDF Number: " + pdf.ID.ToString() + "\r\n" +
                    "File Name: " + pdf.Document!.FullPath + "\r\n" +
                    "Page Count: " + pdf.Document!.PageCount + "\r\n";
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            try
            {
                string text = CommandTextBox.Text.Replace("\r\n", "").Replace("\n", "").Replace(" ", "");
                List<string> split = new List<string>();

                for (int i = 0, before = 0; i < text.Length; i++)
                {
                    if (text[i] == '+' || text[i] == '~' || text[i] == '*')
                    {
                        split.Add(text.Substring(before, i - before));
                        split.Add(text[i].ToString());
                        before = i + 1;
                    }
                    else if (i == text.Length - 1)
                    {
                        split.Add(text.Substring(before, text.Length - before));
                    }
                }

                List<Prompt> prompts = new List<Prompt>();

                foreach (string prompt in split)
                {
                    if (prompt == "+" || prompt == "~" || prompt == "*")
                    {
                        prompts.Add(new Prompt()
                        {
                            Operater = prompt == "+" ? PromptOperater.Add : (prompt == "*" ? PromptOperater.Times : PromptOperater.To)
                        });
                    }
                    else if (prompt.Contains('.'))
                    {
                        prompts.Add(new Prompt()
                        {
                            Operater = PromptOperater.None,
                            PdfID = int.Parse(prompt.Split('.')[0]),
                            PdfPage = int.Parse(prompt.Split(".")[1]),
                        });
                    }
                    else
                    {
                        prompts.Add(new Prompt()
                        {
                            Operater = PromptOperater.Times,
                            PdfPage = int.Parse(prompt),
                        });
                    }
                }

                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "PDF File|*.pdf",
                };

                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }

                PdfDocument document = new PdfDocument();

                for (int i = 0; i < prompts.Count; i++)
                {
                    if (i < prompts.Count - 2 && prompts[i + 1].Operater == PromptOperater.To)
                    {
                        Prompt prompt1 = prompts[i];
                        Prompt prompt2 = prompts[i + 2];

                        if (prompt1.PdfID != prompt2.PdfID)
                        {
                            throw new Exception("A.PdfID is not same B.PdfID in A~B");
                        }

                        for (int p = prompt1.PdfPage - 1; p < prompt2.PdfPage; p++)
                        {
                            PdfPage pdf = (PDFListBox.Items[prompt1.PdfID - 1] as PdfNumbering)!.Document.Pages[p];

                            document.AddPage(pdf);
                        }

                        i += 3;
                    }
                    else if (i < prompts.Count - 2 && prompts[i + 1].Operater == PromptOperater.Times)
                    {
                        Prompt prompt1 = prompts[i];
                        Prompt prompt2 = prompts[i + 2];
                        PdfPage pdf = (PDFListBox.Items[prompt1.PdfID - 1] as PdfNumbering)!.Document.Pages[prompt1.PdfPage - 1];

                        for (int j = 0; j < prompt2.PdfPage; j++)
                        {
                            document.AddPage(pdf);
                        }

                        i += 3;
                    }
                    else if (prompts[i].Operater == PromptOperater.None)
                    {
                        Prompt prompt = prompts[i];
                        PdfPage pdf = (PDFListBox.Items[prompt.PdfID - 1] as PdfNumbering)!.Document.Pages[prompt.PdfPage - 1];

                        document.AddPage(pdf);
                    }
                }

                document.Save(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}