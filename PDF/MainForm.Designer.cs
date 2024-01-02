namespace PDF
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SelectPDFButton = new Button();
            PDFListBox = new ListBox();
            InfoLabel = new Label();
            CommandTextBox = new RichTextBox();
            CreateButton = new Button();
            SuspendLayout();
            // 
            // SelectPDFButton
            // 
            SelectPDFButton.Location = new Point(12, 424);
            SelectPDFButton.Name = "SelectPDFButton";
            SelectPDFButton.Size = new Size(244, 23);
            SelectPDFButton.TabIndex = 1;
            SelectPDFButton.Text = "Select PDF";
            SelectPDFButton.UseVisualStyleBackColor = true;
            SelectPDFButton.Click += SelectPDFButton_Click;
            // 
            // PDFListBox
            // 
            PDFListBox.FormattingEnabled = true;
            PDFListBox.ItemHeight = 15;
            PDFListBox.Location = new Point(12, 12);
            PDFListBox.Name = "PDFListBox";
            PDFListBox.Size = new Size(244, 409);
            PDFListBox.TabIndex = 2;
            PDFListBox.SelectedIndexChanged += PDFListBox_SelectedIndexChanged;
            PDFListBox.Format += PDFListBox_Format;
            // 
            // InfoLabel
            // 
            InfoLabel.AutoSize = true;
            InfoLabel.Location = new Point(262, 12);
            InfoLabel.Name = "InfoLabel";
            InfoLabel.Size = new Size(60, 15);
            InfoLabel.TabIndex = 3;
            InfoLabel.Text = "PDF INFO";
            // 
            // CommandTextBox
            // 
            CommandTextBox.Location = new Point(262, 200);
            CommandTextBox.Name = "CommandTextBox";
            CommandTextBox.Size = new Size(526, 221);
            CommandTextBox.TabIndex = 4;
            CommandTextBox.Text = "";
            // 
            // CreateButton
            // 
            CreateButton.Location = new Point(262, 424);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(526, 23);
            CreateButton.TabIndex = 5;
            CreateButton.Text = "Create From Prompt";
            CreateButton.UseVisualStyleBackColor = true;
            CreateButton.Click += CreateButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(800, 450);
            Controls.Add(CreateButton);
            Controls.Add(CommandTextBox);
            Controls.Add(InfoLabel);
            Controls.Add(PDFListBox);
            Controls.Add(SelectPDFButton);
            Name = "MainForm";
            Text = "PDF Supporter";
            FormClosing += MainForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button SelectPDFButton;
        private ListBox PDFListBox;
        private Label InfoLabel;
        private RichTextBox CommandTextBox;
        private Button CreateButton;
    }
}