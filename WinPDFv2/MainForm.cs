using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Openus.WinPDFv2.Properties;
using Openus.AppPath;

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

        public MainForm()
        {
            /// Set Properties
            {
                Text = "WinPDF v2";

                StartPosition = FormStartPosition.CenterScreen;
                ClientSize = GlobalSetting.Default.WindowSize;
                Icon = GlobalResource.OpenusIcon;

                FormClosed += MainFormClosed;
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
    }
}
