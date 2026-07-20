using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFilterTextReader.Tests.Helpers
{
    internal static class FileHelper
    {
        /// <summary>
        ///     Helper method to create test files with specific encodings
        /// </summary>
        internal static string CreateTestFile(string filename, string content, Encoding encoding)
        {
            var directory = Path.Combine(Path.GetTempPath(), Constants.TestFilesPath);
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllText(filePath, content, encoding);

            return filePath;
        }
    }
}
