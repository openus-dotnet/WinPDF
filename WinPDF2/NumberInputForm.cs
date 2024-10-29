using System;
using System.Windows.Forms;

namespace Openus.WinPDF2
{
    public class NumberInputForm : Form
    {
        private NumericUpDown _inputBox;
        private Button _submitButton;

        public int Submit { get; private set; }

        public NumberInputForm()
        {
            this.Text = "Enter the split unit";
            this.Width = 300;
            this.Height = 150;

            _inputBox = new NumericUpDown()
            {
                Location = new Point(50, 20),
                Width = 200,
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Increment = 1,
            };
            this.Controls.Add(_inputBox);

            _submitButton = new Button
            {
                Text = "Submit",
                Location = new Point(100, 60),
                Width = 80
            };
            _submitButton.Click += SubmitButtonClick;
            this.Controls.Add(_submitButton);
        }

        private void SubmitButtonClick(object? sender, EventArgs e)
        {
            Submit = (int)_inputBox.Value;
            DialogResult = DialogResult.OK;

            Close();
        }
    }
}