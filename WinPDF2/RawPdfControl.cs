using Microsoft.Web.WebView2.WinForms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Openus.WinPDF2
{
    public class RawPdfControl : Panel
    {
        private Button _label;
        private NumericUpDown _fromBox, _toBox;
        private Button _addButton, _deleteButton;
        private TableLayoutPanel _tableLayoutPanel;
        private WebView2 _connectedView;
        private TableLayoutPanel _resultPanel;
        private Label _;

        public int ID { get; private set; }

        public PdfDocument Document { get; private set; }
        public new string Name { get => Text; }

        public static List<RawPdfControl> Pdfs { get; private set; } = new List<RawPdfControl>();

        public RawPdfControl(PdfDocument pdf, int id, WebView2 connectedView, TableLayoutPanel resultPanel)
        {
            /// Set Table
            {
                _tableLayoutPanel = new TableLayoutPanel()
                {
                    Parent = this,
                    Dock = DockStyle.Fill,
                    RowCount = 1,
                    ColumnCount = 6,
                };

                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 1, sizeType: SizeType.Percent));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 50, sizeType: SizeType.Absolute));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 20, sizeType: SizeType.Absolute));
                _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(width: 50, sizeType: SizeType.Absolute));
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
                _resultPanel = resultPanel;

                _label = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = Text,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                };
                _tableLayoutPanel.SetColumn(_label, 0);

                _fromBox = new NumericUpDown()
                {
                    Parent = _tableLayoutPanel,
                    Minimum = 1,
                    Maximum = Document.PageCount,
                    Value = 1,
                    Dock = DockStyle.Fill,
                    Increment = 1,
                };
                _tableLayoutPanel.SetColumn(_fromBox, 1);
                
                _ = new Label()
                {
                    Parent = _tableLayoutPanel,
                    Text = "~",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                };
                _tableLayoutPanel.SetColumn(_, 2);

                _toBox = new NumericUpDown()
                {
                    Parent = _tableLayoutPanel,
                    Minimum = 1,
                    Maximum = Document.PageCount,
                    Value = Document.PageCount,
                    Dock = DockStyle.Fill,
                    Increment = 1,
                };
                _tableLayoutPanel.SetColumn(_toBox, 3);
                
                _addButton = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = "+",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                _tableLayoutPanel.SetColumn(_addButton, 4);

                _deleteButton = new Button()
                {
                    Parent = _tableLayoutPanel,
                    Text = "x",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                _tableLayoutPanel.SetColumn(_deleteButton, 5);

                _label.Click += LabelClick;

                _fromBox.ValueChanged += FromBoxValueChanged;
                _toBox.ValueChanged += ToBoxValueChanged;

                _addButton.Click += AddButtonClick;
                _deleteButton.Click += DeleteButtonClick;
            }
        }

        private void ToBoxValueChanged(object? sender, EventArgs e)
        {
            if (_toBox.Value < _fromBox.Value)
            {
                _toBox.Value = _fromBox.Value;
            }
        }

        private void FromBoxValueChanged(object? sender, EventArgs e)
        {

            if (_toBox.Value < _fromBox.Value)
            {
                _fromBox.Value = _toBox.Value;
            }
        }

        public void DeleteButtonClick(object? sender, EventArgs e)
        {
            TableLayoutPanel parent = Parent as TableLayoutPanel ?? throw new Exception();
            parent.Controls.Remove(this);

            Pdfs.Remove(this);
        }

        public void AddButtonClick(object? sender, EventArgs e)
        {
            int from = (int)_fromBox.Value;
            int to = (int)_toBox.Value;

            PdfDocument newPdf = new PdfDocument();

            for (int i = from - 1; i <= to - 1; i++)
            {
                newPdf.Pages.Add(Document.Pages[i]);
            }

            string newName = new FileInfo(Document.FullPath).Name;
            string newPath = Path.Combine(Program.AppFolder, $"{newName[0..(newName.Length - 4)]} [{from}-{to}].pdf");

            newPdf.Save(newPath);

            ResultPdfControl control = new ResultPdfControl(PdfReader.Open(newPath, PdfDocumentOpenMode.Import), ID, _connectedView)
            {
                Parent = _resultPanel,
                Dock = DockStyle.Fill,
                Height = 30,
            };
            _resultPanel.SetRow(control, ResultPdfControl.Pdfs.Count);

            ResultPdfControl.Pdfs.Add(control);
        }

        private void LabelClick(object? sender, EventArgs e)
        {
            _connectedView.Source = new Uri(Document.FullPath);
        }
    }
}
