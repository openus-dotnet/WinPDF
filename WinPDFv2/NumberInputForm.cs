namespace Openus.WinPDFv2
{
    public class NumberInputForm : Form
    {
        private TableLayoutPanel _tableLayoutPanel;
        private NumericUpDown _inputBox;
        private Button _submitButton;

        public int Submit { get; private set; }

        public NumberInputForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Text = "Enter the split unit";
            ClientSize = new Size(200, 60);
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;

            _tableLayoutPanel = new TableLayoutPanel()
            {
                Parent = this,
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
            };

            _inputBox = new NumericUpDown()
            {
                Parent = _tableLayoutPanel,
                Location = new Point(5, 5),
                Width = 190,
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Increment = 1,
                Dock = DockStyle.Fill,
            };

            _submitButton = new Button
            {
                Parent = _tableLayoutPanel,
                Text = "Submit",
                Location = new Point(5, 10 + _inputBox.Height),
                Width = 190,
                Height = 25,
                Dock = DockStyle.Fill,
            };
            _submitButton.Click += SubmitButtonClick;
        }

        private void SubmitButtonClick(object? sender, EventArgs e)
        {
            Submit = (int)_inputBox.Value;
            DialogResult = DialogResult.OK;

            Close();
        }
    }
}