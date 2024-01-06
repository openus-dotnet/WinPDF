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
using System.Threading;
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
            public static ListBox? AppendPdfListBox { get; set; }

            public PdfWrap(PdfDocument pdf)
            {
                Document = pdf;
                Content = Document.FullPath.Split('\\').Last();
                Foreground = new SolidColorBrush(Colors.White);
                MouseDown += PdfWrap_MouseDown;
                MouseDoubleClick += PdfWrap_MouseDoubleClick;
            }

            private void PdfWrap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    AppendPdfListBox!.Items.Add(new PdfWrap(Document));
                }
            }

            private void PdfWrap_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    MessageBoxResult result = MessageBox.Show("Remove the " + Name + 
                        "?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                
                    if (result == MessageBoxResult.OK)
                    {
                        (Parent as ListBox)!.Items.Remove(this);
                    }
                }
            }
        }

        public PdfDocument? SelectedPdf { get; set; }

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

            PdfWrap.AppendPdfListBox = ResultListBox;
        }

        private void SelectPdfButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() 
            {
                Filter = "PDF File|*.pdf",
                Multiselect = true,
            };

            if (dialog.ShowDialog() == true)
            {
                int sequance = 0;

                CreateProgressBar(dialog.FileNames.Length);

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    string file = dialog.FileNames[i];
                    int x = i;

                    new Thread(() =>
                    {
                        try
                        {
                            PdfDocument pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                            while (sequance != x) ;

                            lock (SelectPdfListBox)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    PdfWrap wrap = new PdfWrap(pdf);
                                    SelectPdfListBox.Items.Add(wrap);

                                    ProgressingBar();
                                });

                                sequance++;
                            }
                        }
                        catch (Exception ex)
                        {
                            while (sequance != x) ;

                            lock (SelectPdfListBox)
                            {
                                sequance++;
                            }

                            MessageBox.Show(ex.Message + "\r\n" + file, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }).Start();
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfWrap? item = (sender as ListBox)!.SelectedItem as PdfWrap;
            
            if (item != null)
            {
                SetPdfLabel(item);
                SelectedPdf = item.Document;
            }
        }

        private void ListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ListBox? listBox = sender as ListBox;

            if (listBox != null)
            {
                listBox.SelectedItem = null;
            }
        }

        private void FromToTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FromToTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PartedPaging();
            }
        }

        private void FromToTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox != null && int.TryParse(textBox.Text, out int a) && SelectedPdf != null)
            {
                if (e.Delta > 0)
                {
                    if (a + 1 <= SelectedPdf.PageCount)
                    {
                        textBox.Text = (a + 1).ToString();
                    }
                }
                else
                {
                    if (a - 1 > 0)
                    {
                        textBox.Text = (a - 1).ToString();
                    }
                }
            }
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPdf == null)
            {
                MessageBox.Show("You do not select PDF", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                return;
            }
            
            if (PartedPaging() == true)
            {
                ResultListBox.Items.Add(new PdfWrap(PdfReader.Open(WebView.Source.LocalPath, PdfDocumentOpenMode.Import)));
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
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

            try
            {
                pdf.Save(path);

                PreviewWindow preview = new PreviewWindow();
                preview.PreviewWebView.Source = new Uri(path);
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMergeButton_Click(object sender, RoutedEventArgs e)
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
                try
                {
                    pdf.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveSplitButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog() 
            {
                Filter = "PDF File|*.pdf",
            };

            int i = 1;

            if (dialog.ShowDialog() == true)
            {
                foreach (PdfWrap wrap in ResultListBox.Items)
                {
                    foreach (PdfPage page in wrap.Document.Pages)
                    {
                        PdfDocument pdf = new PdfDocument();

                        pdf.Pages.Add(page);

                        try
                        {
                            pdf.Save(dialog.FileName[0..(dialog.FileName.Length - 4)] + (i++) + ".pdf");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void CreateProgressBar(int maximum)
        {
            PublicProgressBar.Maximum = maximum;
            PublicProgressBar.Visibility = Visibility.Visible;
            PublicProgressBar.Value = 0;
        }

        private void ProgressingBar(int value = 1)
        {
            PublicProgressBar.Value += value;

            if (PublicProgressBar.Value == PublicProgressBar.Maximum)
            {
                PublicProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void SetPdfLabel(PdfWrap item)
        {
            FromTextBox.Text = "1";
            ToTextBox.Text = item.Document.PageCount.ToString();

            string line = "\r\n";

            PdfInfoLabel.Content =
                "Selected PDF Information" + line + line +
                "PDF Name: " + item.Name + line +
                "PDF Location: " + item.Document.FullPath[0..(item.Document.FullPath.Length - item.Name.Length)] + line +
                "PDF Page Count: " + item.Document.PageCount + line;
            WebView.Source = new Uri(item.Document.FullPath);
        }

        private bool PartedPaging()
        {
            int resultFrom = int.Parse(FromTextBox.Text) - 1;
            int resultTo = int.Parse(ToTextBox.Text) - 1;

            if (SelectPdfListBox.SelectedItem != null)
            {
                PdfWrap? wrap = SelectPdfListBox.SelectedItem as PdfWrap;

                if (resultFrom < 0 || resultFrom > wrap!.Document.PageCount || resultTo < 0 || resultTo > wrap!.Document.PageCount)
                {
                    MessageBox.Show((resultFrom + 1) + "~" + (resultTo + 1) + " is invalid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
                else if (int.Parse(FromTextBox.Text) > int.Parse(ToTextBox.Text))
                {
                    MessageBox.Show("From page is not over than to page.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                    return false;
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

                    return true;
                }
            }

            return false;
        }
    }
}
