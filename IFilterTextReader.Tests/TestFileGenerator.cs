//
// TestFileGenerator.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2024 Magic-Sessions. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace IFilterTextReader.Tests;

/// <summary>
///     Utility class to generate test files for manual verification
/// </summary>
public class TestFileGenerator
{
    private readonly ITestOutputHelper _output;

    public TestFileGenerator(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Skip = "Manual test file generation - run manually when needed")]
    public void GenerateAllEncodingTestFiles()
    {
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
        Directory.CreateDirectory(outputDir);

        var testContent = @"English: test
Japanese: テスト
Chinese: 测试
Korean: 테스트
Russian: тест
Arabic: اختبار
Hebrew: מִבְחָן";

        // UTF-8 with BOM
        CreateFile(Path.Combine(outputDir, "utf8_with_bom.txt"), testContent, Encoding.UTF8);
        
        // UTF-8 without BOM
        CreateFile(Path.Combine(outputDir, "utf8_without_bom.txt"), testContent, new UTF8Encoding(false));
        
        // UTF-16 LE
        CreateFile(Path.Combine(outputDir, "utf16_le.txt"), testContent, Encoding.Unicode);
        
        // UTF-16 BE
        CreateFile(Path.Combine(outputDir, "utf16_be.txt"), testContent, Encoding.BigEndianUnicode);
        
        // Shift-JIS (Japanese only)
        try
        {
            var shiftJis = Encoding.GetEncoding("shift_jis");
            CreateFile(Path.Combine(outputDir, "shift_jis.txt"), "Japanese: テスト", shiftJis);
        }
        catch
        {
            _output.WriteLine("Shift-JIS encoding not available");
        }

        // ASCII
        CreateFile(Path.Combine(outputDir, "ascii.txt"), "English: test", Encoding.ASCII);

        _output.WriteLine($"Test files generated in: {outputDir}");
    }

    private void CreateFile(string path, string content, Encoding encoding)
    {
        File.WriteAllText(path, content, encoding);
        _output.WriteLine($"Created: {Path.GetFileName(path)} ({encoding.EncodingName})");
    }
}
