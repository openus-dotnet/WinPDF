using Microsoft.Web.WebView2.WinForms;
using PdfSharp.Pdf;

namespace WinPDF2
{
    public class ResultPdfControl : Panel
    {
        private Button _label;
        private TableLayoutPanel _tableLayoutPanel;
        private WebView2 _connectedView;
        private Button _deleteButton;
        private Button _upButton;
        private Button _downButton;

        public int ID { get; private set; }

        public PdfDocument Document { get; private set; }
        public new string Name { get => Text; }

        public static List<ResultPdfControl> Pdfs { get; private set; } = new List<ResultPdfControl>();

        public ResultPdfControl(PdfDocument pdf, int id, WebView2 connectedView)
        {
            /// Set Table
            {
                _tableLayoutPanel = new TableLayoutPanel()
                {
                    Parent = this,
                    Dock = DockStyle.Fill,
                    RowCount = 1,
                    ColumnCount = 4,
                };

                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 30, sizeType: SizeType.Absolute));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 30, sizeType: SizeType.Absolute));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 30, sizeType: SizeType.Absolute));
            }

            /// Set Properties
            {
                Size = new Size(200, 30);
                Document = pdf;
                Text = new FileInfo(Document.FullPath).Name;
                ID = id;

                _connectedView = connectedView;

                _label = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = Text,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                };
                _tableLayoutPanel.SetColumn(_label, 0);

                _upButton = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = "↑",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                _tableLayoutPanel.SetColumn(_upButton, 1);

                _downButton = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = "↓",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                _tableLayoutPanel.SetColumn(_downButton, 2);

                _deleteButton = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = "x",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                _tableLayoutPanel.SetColumn(_deleteButton, 3);

                _label.Click += LabelClick;
                _deleteButton.Click += DeleteButtonClick;
                _upButton.Click += UpButtonClick;
                _downButton.Click += DownButtonClick;
            }
        }

        private void DownButtonClick(object? sender, EventArgs e)
        {
            TableLayoutPanel parent = (TableLayoutPanel)Parent!;
            int idx = Pdfs.IndexOf(this);

            if (idx >= Pdfs.Count - 1)
            {
                return;
            }

            (Pdfs[idx], Pdfs[idx + 1]) = (Pdfs[idx + 1], Pdfs[idx]);

            parent.Controls.Clear();

            for (int i = 0; i < Pdfs.Count; i++)
            {
                Pdfs[i].Parent = parent;
                parent.SetRow(Pdfs[i], i);
            }
        }

        private void UpButtonClick(object? sender, EventArgs e)
        {
            TableLayoutPanel parent = (TableLayoutPanel)Parent!;
            int idx = Pdfs.IndexOf(this);

            if (idx <= 0)
            {
                return;
            }

            (Pdfs[idx], Pdfs[idx - 1]) = (Pdfs[idx - 1], Pdfs[idx]);

            parent.Controls.Clear();

            for (int i = 0; i < Pdfs.Count; i++)
            {
                Pdfs[i].Parent = parent;
                parent.SetRow(Pdfs[i], i);
            }
        }

        public void DeleteButtonClick(object? sender, EventArgs e)
        {
            TableLayoutPanel parent = Parent as TableLayoutPanel ?? throw new Exception();
            parent.Controls.Remove(this);

            Pdfs.Remove(this);
        }

        private void LabelClick(object? sender, EventArgs e)
        {
            _connectedView.Source = new Uri(Document.FullPath);
        }
    }
}
