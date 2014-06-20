using System;
using System.IO;
using System.Windows.Forms;
using Email2Storage.Modules.Readers.IFilterTextReader;

/*
   Copyright 2014 Kees van Spelde

   Licensed under The Code Project Open License (CPOL) 1.02;
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.codeproject.com/info/cpol10.aspx

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
                SelectedFileLabel.Text = "File: " + openFileDialog1.FileName;

                try
                {
                    TextReader reader = new FilterReader(openFileDialog1.FileName);
                    using (reader)
                        FilterTextBox.Text = reader.ReadToEnd();
                        
                }
                catch (Exception ex)
                {
                    FilterTextBox.Text = ex.Message;
                }
            }
        }
    }
}
