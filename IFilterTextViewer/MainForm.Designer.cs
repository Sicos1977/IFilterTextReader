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
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.SelectButton = new System.Windows.Forms.Button();
            this.TextToFindTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FindTextButton = new System.Windows.Forms.Button();
            this.FindWithRegexButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TextToFindWithRegexTextBox = new System.Windows.Forms.TextBox();
            this.FileLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.AcceptsReturn = true;
            this.FilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterTextBox.Location = new System.Drawing.Point(15, 183);
            this.FilterTextBox.Multiline = true;
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(1042, 515);
            this.FilterTextBox.TabIndex = 0;
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(15, 12);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(149, 42);
            this.SelectButton.TabIndex = 2;
            this.SelectButton.Text = "Select file";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // TextToFindTextBox
            // 
            this.TextToFindTextBox.Enabled = false;
            this.TextToFindTextBox.Location = new System.Drawing.Point(255, 77);
            this.TextToFindTextBox.Name = "TextToFindTextBox";
            this.TextToFindTextBox.Size = new System.Drawing.Size(668, 31);
            this.TextToFindTextBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(124, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Text to find:";
            // 
            // FindTextButton
            // 
            this.FindTextButton.Enabled = false;
            this.FindTextButton.Location = new System.Drawing.Point(929, 73);
            this.FindTextButton.Name = "FindTextButton";
            this.FindTextButton.Size = new System.Drawing.Size(128, 39);
            this.FindTextButton.TabIndex = 5;
            this.FindTextButton.Text = "Find";
            this.FindTextButton.UseVisualStyleBackColor = true;
            this.FindTextButton.Click += new System.EventHandler(this.FindTextButton_Click);
            // 
            // FindWithRegexButton
            // 
            this.FindWithRegexButton.Enabled = false;
            this.FindWithRegexButton.Location = new System.Drawing.Point(929, 121);
            this.FindWithRegexButton.Name = "FindWithRegexButton";
            this.FindWithRegexButton.Size = new System.Drawing.Size(128, 39);
            this.FindWithRegexButton.TabIndex = 8;
            this.FindWithRegexButton.Text = "Find";
            this.FindWithRegexButton.UseVisualStyleBackColor = true;
            this.FindWithRegexButton.Click += new System.EventHandler(this.FindWithRegexButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(229, 25);
            this.label2.TabIndex = 7;
            this.label2.Text = "Text to find with regex:";
            // 
            // TextToFindWithRegexTextBox
            // 
            this.TextToFindWithRegexTextBox.Enabled = false;
            this.TextToFindWithRegexTextBox.Location = new System.Drawing.Point(255, 122);
            this.TextToFindWithRegexTextBox.Name = "TextToFindWithRegexTextBox";
            this.TextToFindWithRegexTextBox.Size = new System.Drawing.Size(668, 31);
            this.TextToFindWithRegexTextBox.TabIndex = 6;
            // 
            // FileLabel
            // 
            this.FileLabel.AutoSize = true;
            this.FileLabel.Location = new System.Drawing.Point(182, 21);
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(234, 25);
            this.FileLabel.TabIndex = 9;
            this.FileLabel.Text = "Please select a file first";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1069, 710);
            this.Controls.Add(this.FileLabel);
            this.Controls.Add(this.FindWithRegexButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TextToFindWithRegexTextBox);
            this.Controls.Add(this.FindTextButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextToFindTextBox);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.FilterTextBox);
            this.Name = "MainForm";
            this.Text = "IFilter Text Viewer";
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
    }
}

