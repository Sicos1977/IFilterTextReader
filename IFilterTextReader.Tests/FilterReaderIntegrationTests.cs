//
// FilterReaderIntegrationTests.cs
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

using FluentAssertions;
using IFilterTextReader.Tests.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace IFilterTextReader.Tests;

/// <summary>
///     Integration tests for real-world scenarios
/// </summary>
public class FilterReaderIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public FilterReaderIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Scenario_JapaneseLogFile_ShouldBeSearchable()
    {
        // Arrange
        var logContent = @"2024-01-15 10:30:45 [INFO] アプリケーション起動
2024-01-15 10:30:46 [INFO] データベース接続成功
2024-01-15 10:31:00 [ERROR] エラーが発生しました: テスト
2024-01-15 10:31:01 [INFO] 処理完了";

        var filePath = FileHelper.CreateTestFile("japanese_app.log", logContent, new UTF8Encoding(false));
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act - Use FilterReader directly instead of IFilterTextViewer.Reader
        bool foundError = false;
        bool foundTest = false;

        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.IndexOf("エラーが発生しました", StringComparison.InvariantCultureIgnoreCase) >= 0)
                        foundError = true;
                    if (line.IndexOf("テスト", StringComparison.InvariantCultureIgnoreCase) >= 0)
                        foundTest = true;
                }
            }
        }

        // Assert
        foundError.Should().BeTrue("log should contain error message");
        foundTest.Should().BeTrue("log should contain test word");

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void Scenario_MixedEncodingFiles_ShouldAllBeReadable()
    {
        // Arrange
        var testData = new[]
        {
            ("utf8_bom.txt", "UTF-8 テスト", Encoding.UTF8),
            ("utf8_nobom.txt", "UTF-8 テスト", new UTF8Encoding(false)),
            ("utf16.txt", "UTF-16 テスト", Encoding.Unicode),
        };

        var options = new FilterReaderOptions { UseEncodingDetection = true };

        foreach (var (filename, content, encoding) in testData)
        {
            // Arrange
            var filePath = FileHelper.CreateTestFile(filename, content, encoding);

            // Act
            string result;
            using (var reader = new FilterReader(filePath, filterReaderOptions: options))
            {
                result = reader.ReadToEnd();
            }

            // Assert
            result.Trim().Should().Be(content, $"{filename} with encoding {encoding.EncodingName} should be readable");

            _output.WriteLine($"✓ Successfully read {filename} ({encoding.EncodingName})");

            // Cleanup
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Scenario_LargeFileWithJapaneseText_ShouldHandleEfficiently()
    {
        // Arrange
        var sb = new StringBuilder();
        for (var i = 0; i < 1000; i++)
        {
            sb.AppendLine($"行 {i}: これはテストです。日本語のテキストを含む大きなファイル。");
        }

        var filePath = FileHelper.CreateTestFile("large_japanese.txt", sb.ToString(), new UTF8Encoding(false));
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        var lineCount = 0;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            while (reader.ReadLine() != null)
            {
                lineCount++;
            }
        }

        // Assert
        lineCount.Should().Be(1000, "should read all 1000 lines");

        _output.WriteLine($"Successfully processed {lineCount} lines of Japanese text");

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void Scenario_CorruptedEncoding_ShouldFallbackGracefully()
    {
        // Arrange - Create a file with invalid UTF-8 sequences
        var filePath = Path.Combine(Path.GetTempPath(), Constants.TestFilesPath, "corrupted.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var invalidBytes = new byte[] { 0xFF, 0xFE, 0x00, 0x54, 0x65, 0x73, 0x74 };
        File.WriteAllBytes(filePath, invalidBytes);

        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act - Should not throw
        var exception = Record.Exception(() =>
        {
            using var reader = new FilterReader(filePath, filterReaderOptions: options);
            var result = reader.ReadToEnd();
            _output.WriteLine($"Read result from corrupted file: {result}");
        });

        // Assert
        exception.Should().BeNull("should handle corrupted files gracefully");

        // Cleanup
        File.Delete(filePath);
    }
}
