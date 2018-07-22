namespace IFilterTextViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.SelectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FindTextButton = new System.Windows.Forms.Button();
            this.FindWithRegexButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.FileLabel = new System.Windows.Forms.Label();
            this.TimeoutOptionsComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TimeoutTextBox = new System.Windows.Forms.TextBox();
            this.ReadIntoMemoryCheckBox = new System.Windows.Forms.CheckBox();
            this.DisableEmbeddedContentCheckBox = new System.Windows.Forms.CheckBox();
            this.IncludePropertiesCheckBox = new System.Windows.Forms.CheckBox();
            this.TextToFindWithRegexTextBox = new System.Windows.Forms.TextBox();
            this.TextToFindTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.AcceptsReturn = true;
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterTextBox.Location = new System.Drawing.Point(8, 133);
            this.FilterTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FilterTextBox.Multiline = true;
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.FilterTextBox.Size = new System.Drawing.Size(523, 308);
            this.FilterTextBox.TabIndex = 11;
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(8, 6);
            this.SelectButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(74, 22);
            this.SelectButton.TabIndex = 1;
            this.SelectButton.Text = "Select file";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 88);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Text to find:";
            // 
            // FindTextButton
            // 
            this.FindTextButton.Enabled = false;
            this.FindTextButton.Location = new System.Drawing.Point(465, 84);
            this.FindTextButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FindTextButton.Name = "FindTextButton";
            this.FindTextButton.Size = new System.Drawing.Size(64, 20);
            this.FindTextButton.TabIndex = 8;
            this.FindTextButton.Text = "Find";
            this.FindTextButton.UseVisualStyleBackColor = true;
            this.FindTextButton.Click += new System.EventHandler(this.FindTextButton_Click);
            // 
            // FindWithRegexButton
            // 
            this.FindWithRegexButton.Enabled = false;
            this.FindWithRegexButton.Location = new System.Drawing.Point(465, 109);
            this.FindWithRegexButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FindWithRegexButton.Name = "FindWithRegexButton";
            this.FindWithRegexButton.Size = new System.Drawing.Size(64, 20);
            this.FindWithRegexButton.TabIndex = 10;
            this.FindWithRegexButton.Text = "Find";
            this.FindWithRegexButton.UseVisualStyleBackColor = true;
            this.FindWithRegexButton.Click += new System.EventHandler(this.FindWithRegexButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 113);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Text to find with regex:";
            // 
            // FileLabel
            // 
            this.FileLabel.Location = new System.Drawing.Point(91, 11);
            this.FileLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(438, 17);
            this.FileLabel.TabIndex = 9;
            this.FileLabel.Text = "Please select a file first";
            // 
            // TimeoutOptionsComboBox
            // 
            this.TimeoutOptionsComboBox.FormattingEnabled = true;
            this.TimeoutOptionsComboBox.Items.AddRange(new object[] {
            "No timeout",
            "Timeout only",
            "Raise exception"});
            this.TimeoutOptionsComboBox.Location = new System.Drawing.Point(127, 61);
            this.TimeoutOptionsComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TimeoutOptionsComboBox.Name = "TimeoutOptionsComboBox";
            this.TimeoutOptionsComboBox.Size = new System.Drawing.Size(118, 21);
            this.TimeoutOptionsComboBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(247, 63);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Timeout (ms):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Timeout option:";
            // 
            // TimeoutTextBox
            // 
            this.TimeoutTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::IFilterTextViewer.Properties.Settings.Default, "TimeoutTextBoxValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.TimeoutTextBox.Location = new System.Drawing.Point(322, 61);
            this.TimeoutTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TimeoutTextBox.Name = "TimeoutTextBox";
            this.TimeoutTextBox.Size = new System.Drawing.Size(142, 20);
            this.TimeoutTextBox.TabIndex = 6;
            this.TimeoutTextBox.Text = global::IFilterTextViewer.Properties.Settings.Default.TimeoutTextBoxValue;
            // 
            // ReadIntoMemoryCheckBox
            // 
            this.ReadIntoMemoryCheckBox.AutoSize = true;
            this.ReadIntoMemoryCheckBox.Checked = global::IFilterTextViewer.Properties.Settings.Default.ReadIntoMemory;
            this.ReadIntoMemoryCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::IFilterTextViewer.Properties.Settings.Default, "ReadIntoMemory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ReadIntoMemoryCheckBox.Location = new System.Drawing.Point(419, 37);
            this.ReadIntoMemoryCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ReadIntoMemoryCheckBox.Name = "ReadIntoMemoryCheckBox";
            this.ReadIntoMemoryCheckBox.Size = new System.Drawing.Size(111, 17);
            this.ReadIntoMemoryCheckBox.TabIndex = 4;
            this.ReadIntoMemoryCheckBox.Text = "Read into memory";
            this.ReadIntoMemoryCheckBox.UseVisualStyleBackColor = true;
            // 
            // DisableEmbeddedContentCheckBox
            // 
            this.DisableEmbeddedContentCheckBox.AutoSize = true;
            this.DisableEmbeddedContentCheckBox.Checked = global::IFilterTextViewer.Properties.Settings.Default.DisableEmbeddedContent;
            this.DisableEmbeddedContentCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::IFilterTextViewer.Properties.Settings.Default, "DisableEmbeddedContent", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DisableEmbeddedContentCheckBox.Location = new System.Drawing.Point(127, 37);
            this.DisableEmbeddedContentCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DisableEmbeddedContentCheckBox.Name = "DisableEmbeddedContentCheckBox";
            this.DisableEmbeddedContentCheckBox.Size = new System.Drawing.Size(153, 17);
            this.DisableEmbeddedContentCheckBox.TabIndex = 2;
            this.DisableEmbeddedContentCheckBox.Text = "Disable embedded content";
            this.DisableEmbeddedContentCheckBox.UseVisualStyleBackColor = true;
            // 
            // IncludePropertiesCheckBox
            // 
            this.IncludePropertiesCheckBox.AutoSize = true;
            this.IncludePropertiesCheckBox.Checked = global::IFilterTextViewer.Properties.Settings.Default.IncludeProperties;
            this.IncludePropertiesCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::IFilterTextViewer.Properties.Settings.Default, "IncludeProperties", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.IncludePropertiesCheckBox.Location = new System.Drawing.Point(293, 37);
            this.IncludePropertiesCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.IncludePropertiesCheckBox.Name = "IncludePropertiesCheckBox";
            this.IncludePropertiesCheckBox.Size = new System.Drawing.Size(110, 17);
            this.IncludePropertiesCheckBox.TabIndex = 3;
            this.IncludePropertiesCheckBox.Text = "Include properties";
            this.IncludePropertiesCheckBox.UseVisualStyleBackColor = true;
            // 
            // TextToFindWithRegexTextBox
            // 
            this.TextToFindWithRegexTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::IFilterTextViewer.Properties.Settings.Default, "TextToFindWithRegex", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.TextToFindWithRegexTextBox.Enabled = false;
            this.TextToFindWithRegexTextBox.Location = new System.Drawing.Point(128, 110);
            this.TextToFindWithRegexTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextToFindWithRegexTextBox.Name = "TextToFindWithRegexTextBox";
            this.TextToFindWithRegexTextBox.Size = new System.Drawing.Size(336, 20);
            this.TextToFindWithRegexTextBox.TabIndex = 9;
            this.TextToFindWithRegexTextBox.Text = global::IFilterTextViewer.Properties.Settings.Default.TextToFindWithRegex;
            // 
            // TextToFindTextBox
            // 
            this.TextToFindTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::IFilterTextViewer.Properties.Settings.Default, "TextToFind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.TextToFindTextBox.Enabled = false;
            this.TextToFindTextBox.Location = new System.Drawing.Point(128, 86);
            this.TextToFindTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextToFindTextBox.Name = "TextToFindTextBox";
            this.TextToFindTextBox.Size = new System.Drawing.Size(336, 20);
            this.TextToFindTextBox.TabIndex = 7;
            this.TextToFindTextBox.Text = global::IFilterTextViewer.Properties.Settings.Default.TextToFind;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 450);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TimeoutOptionsComboBox);
            this.Controls.Add(this.TimeoutTextBox);
            this.Controls.Add(this.ReadIntoMemoryCheckBox);
            this.Controls.Add(this.DisableEmbeddedContentCheckBox);
            this.Controls.Add(this.IncludePropertiesCheckBox);
            this.Controls.Add(this.FileLabel);
            this.Controls.Add(this.FindWithRegexButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TextToFindWithRegexTextBox);
            this.Controls.Add(this.FindTextButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextToFindTextBox);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.FilterTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.Text = "IFilter Text Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.TextBox TextToFindTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button FindTextButton;
        private System.Windows.Forms.Button FindWithRegexButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextToFindWithRegexTextBox;
        private System.Windows.Forms.Label FileLabel;
        private System.Windows.Forms.CheckBox IncludePropertiesCheckBox;
        private System.Windows.Forms.CheckBox DisableEmbeddedContentCheckBox;
        private System.Windows.Forms.CheckBox ReadIntoMemoryCheckBox;
        private System.Windows.Forms.TextBox TimeoutTextBox;
        private System.Windows.Forms.ComboBox TimeoutOptionsComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

