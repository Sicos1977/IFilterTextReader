using System;
using System.Diagnostics;
using System.Windows.Forms;

/*
   Copyright 2013-2015 Kees van Spelde

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

using IFilterTextReader;
using IFilterTextViewer.Properties;

namespace IFilterTextViewer
{
    public partial class MainForm : Form
    {
        #region Fields
        /// <summary>
        /// Make a job object to sandbox the IFilter code
        /// </summary>
        private readonly Job _job = new Job();
        #endregion

        #region GetInnerException
        /// <summary>
        /// Geeft de volledige inner exceptie
        /// </summary>
        /// <param name="exception">exceptie object</param>
        /// <returns>inhoud van de inner exceptie</returns>
        public static string GetInnerException(Exception exception)
        {
            var result = string.Empty;

            if (exception == null) return result;
            result = exception.Message + Environment.NewLine;
            if (exception.InnerException != null)
                result += GetInnerException(exception.InnerException);
            return result;
        }
        #endregion

        public MainForm()
        {
            InitializeComponent();

            // Add the current process to the sandbox
            _job.AddProcess(Process.GetCurrentProcess().Handle);
        }

        private void DisableInput()
        {
            SelectButton.Enabled = false;
            FindTextButton.Enabled = false;
            FindWithRegexButton.Enabled = false;
            TextToFindTextBox.Enabled = false;
            TextToFindWithRegexTextBox.Enabled = false;
            DisableEmbeddedContentCheckBox.Enabled = false;
            IncludePropertiesCheckBox.Enabled = false;
            FilterTextBox.Clear();
        }

        private void EnableInput()
        {
            SelectButton.Enabled = true;
            FindTextButton.Enabled = true;
            FindWithRegexButton.Enabled = true;
            TextToFindTextBox.Enabled = true;
            TextToFindWithRegexTextBox.Enabled = true;
            DisableEmbeddedContentCheckBox.Enabled = true;
            IncludePropertiesCheckBox.Enabled = true;            
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
                FileLabel.Text = openFileDialog1.FileName;
                FindTextButton.Enabled = true;
                TextToFindTextBox.Enabled = true;
                FindWithRegexButton.Enabled = true;
                TextToFindWithRegexTextBox.Enabled = true;

                try
                {
                    DisableInput();

                    FilterTextBox.AppendText("*** Processing file '" + openFileDialog1.FileName + "' ***" + Environment.NewLine + Environment.NewLine);
                    Application.DoEvents();

                    using (
                        var reader = new FilterReader(openFileDialog1.FileName, 
                            string.Empty, 
                            DisableEmbeddedContentCheckBox.Checked,
                            IncludePropertiesCheckBox.Checked))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            FilterTextBox.AppendText(line + Environment.NewLine);
                            Application.DoEvents();
                        }

                        FilterTextBox.AppendText(Environment.NewLine + "*** DONE ***" + Environment.NewLine);
                        Application.DoEvents();
                    }
                }
                catch (Exception exception)
                {
                    DisableInput();
                    FilterTextBox.Text = exception.StackTrace + Environment.NewLine + GetInnerException(exception);
                }
                finally
                {
                    EnableInput();
                }
            }
        }

        private void FindTextButton_Click(object sender, EventArgs e)
        {
            try
            {
                DisableInput();
                if (new Reader().FileContainsText(FileLabel.Text, TextToFindTextBox.Text))
                    MessageBox.Show("Text '" + TextToFindTextBox.Text + "' found inside the file");
                else
                    MessageBox.Show("Text '" + TextToFindTextBox.Text + "' not found inside the file");
            }
            catch (Exception exception)
            {
                FilterTextBox.Text = exception.StackTrace + Environment.NewLine + GetInnerException(exception);
            }
            finally
            {
                EnableInput();
            }
        }

        private void FindWithRegexButton_Click(object sender, EventArgs e)
        {
            try
            {
                var matches = new Reader().GetRegexMatchesFromFile(FileLabel.Text, TextToFindWithRegexTextBox.Text);
                if (matches != null)
                    FilterTextBox.Lines = matches;
            }
            catch (Exception exception)
            {
                FilterTextBox.Text = exception.StackTrace + Environment.NewLine + GetInnerException(exception);
            }
            finally
            {
                EnableInput();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
