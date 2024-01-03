using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinPDF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class PdfWrap : Label
        {
            public PdfDocument Document { get; set; }
            public new string Name => Document.FullPath.Split('\\').Last();

            public PdfWrap(PdfDocument pdf)
            {
                Document = pdf;
                Content = Document.FullPath.Split('\\').Last();
                Foreground = new SolidColorBrush(Colors.White);
                MouseDown += PdfWrap_MouseDown;
            }

            private void PdfWrap_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    MessageBoxResult result = MessageBox.Show("Remove the " + Name + "?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                
                    if (result == MessageBoxResult.OK)
                    {
                        (Parent as ListBox)!.Items.Remove(this);
                    }
                }
            }
        }

        public List<PdfWrap> Documents { get; set; } = new List<PdfWrap>();

        public MainWindow()
        {
            InitializeComponent();

            if (Directory.Exists(Environment.CurrentDirectory + "\\temp") == false)
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\temp");
            }
            else
            {
                DirectoryInfo info = new DirectoryInfo(Environment.CurrentDirectory + "\\temp");

                foreach (FileInfo file in info.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        private void PdfAddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() 
            {
                Filter = "PDF File|*.pdf",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == true)
            {
                PdfListBox.Items.Clear();

                foreach (var file in dialog.FileNames)
                {
                    try
                    {
                        PdfDocument pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                        PdfWrap wrap = new PdfWrap(pdf);
                        Documents.Add(wrap);
                        PdfListBox.Items.Add(wrap);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Invalid PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void PdfListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfWrap? item = (sender as ListBox)!.SelectedItem as PdfWrap;
            
            if (item != null)
            {
                SetPdfLabel(item);
            }
        }

        private void SetPdfLabel(PdfWrap item)
        {
            FromTextBox.Text = "1";
            ToTextBox.Text = item.Document.PageCount.ToString();

            string line = "\r\n";

            PdfInfoLabel.Content =
                "Selected PDF Information" + line +
                "PDF Name: " + item.Name + line +
                "PDF Location: " + item.Document.FullPath[0..(item.Document.FullPath.Length - item.Name.Length)] + line +
                "PDF Page Count: " + item.Document.PageCount + line;
            WebView.Source = new Uri(item.Document.FullPath);
        }

        private void FromToTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FromToTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PartedPaging(sender);
        }

        private void FromToTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PartedPaging(sender);
            }
        }

        private void PartedPaging(object sender)
        {
            TextBox? box = sender as TextBox;

            if (box != null)
            {
                bool tryParse = int.TryParse(box.Text, out int result);
                result--;

                if (tryParse == true)
                {
                    if (PdfListBox.SelectedItem != null)
                    {
                        PdfWrap? wrap = PdfListBox.SelectedItem as PdfWrap;

                        if (result < 0 || result > wrap!.Document.PageCount)
                        {
                            MessageBox.Show((result + 1) + " is invalid number.", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else if (int.Parse(FromTextBox.Text) > int.Parse(ToTextBox.Text))
                        {
                            MessageBox.Show("From page is not over than to page.", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            PdfDocument document = new PdfDocument();
                            string path = Environment.CurrentDirectory + "\\temp\\" + wrap.Name[0..(wrap.Name.Length - 4)] 
                                + "(" + FromTextBox.Text + "-" + ToTextBox.Text + ").pdf";

                            for (int i = int.Parse(FromTextBox.Text) - 1; i < int.Parse(ToTextBox.Text); i++)
                            {
                                document.Pages.Add(wrap.Document.Pages[i]);
                            }

                            document.Save(path);

                            WebView.Source = new Uri(path);
                        }
                    }
                }
            }
        }

        private void AddResultButton_Click(object sender, RoutedEventArgs e)
        {
            ResultListBox.Items.Add(new PdfWrap(PdfReader.Open(WebView.Source.LocalPath, PdfDocumentOpenMode.Import)));
        }

        private void ResultPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            PdfDocument pdf = new PdfDocument();
            string path = Environment.CurrentDirectory + "\\temp\\" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".pdf";

            foreach (PdfWrap wrap in ResultListBox.Items)
            {
                foreach (PdfPage page in wrap.Document.Pages)
                {
                    pdf.Pages.Add(page);
                }
            }

            pdf.Save(path);
            WebView.Source = new Uri(path);
            SetPdfLabel(new PdfWrap(PdfReader.Open(path)));
        }

        private void ResultSaveButton_Click(object sender, RoutedEventArgs e)
        {
            PdfDocument pdf = new PdfDocument();

            foreach (PdfWrap wrap in ResultListBox.Items)
            {
                foreach (PdfPage page in wrap.Document.Pages)
                {
                    pdf.Pages.Add(page);
                }
            }

            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "PDF File|*.pdf",
            };

            if (dialog.ShowDialog() == true)
            {
                pdf.Save(dialog.FileName);
            }
        }

        private void PdfListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ListBox? listBox = sender as ListBox;

            if (listBox != null)
            {
                listBox.SelectedItem = null;
            }
        }
    }
}
