using System;
using System.IO;
using System.Windows.Forms;
using Email2Storage.Modules.Readers.IFilterTextReader;

namespace IFilterTextViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            // Create an instance of the open file dialog box.
            var openFileDialog1 = new OpenFileDialog
            {
                // ReSharper disable once LocalizableElement
                Filter = "Alle files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            // Process input if the user clicked OK.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    TextReader reader = new FilterReader(openFileDialog1.FileName);
                    using (reader)
                    {
                        FilterTextBox.Text = reader.ReadToEnd();
                        SelectedFileLabel.Text = "File: " + openFileDialog1.FileName;
                    }
                }
                catch (Exception ex)
                {
                    FilterTextBox.Text = ex.Message;
                }
            }
        }
    }
}
