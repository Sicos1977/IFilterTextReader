//
// FilterReaderEncodingTests.cs
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
using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions;
using IFilterTextReader.Tests.Helpers;
using Xunit;

namespace IFilterTextReader.Tests;

/// <summary>
///     Tests for FilterReader with different file encodings
/// </summary>
public class FilterReaderEncodingTests
{
    private const string JapaneseText = "テスト";
    private const string ChineseText = "测试";
    private const string KoreanText = "테스트";
    private const string MixedText = "テスト test 测试 тест";

    [Fact]
    public void FilterReader_Utf8WithBom_WithEncodingDetection_ShouldReadJapaneseCorrectly()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("japanese_utf8_bom.txt", JapaneseText, Encoding.UTF8);
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(JapaneseText);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_Utf8WithoutBom_WithEncodingDetection_ShouldReadJapaneseCorrectly()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("japanese_utf8_nobom.txt", JapaneseText, new UTF8Encoding(false));
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(JapaneseText);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_ShiftJis_WithEncodingDetection_ShouldReadJapaneseCorrectly()
    {
        // Arrange
        Encoding shiftJis;
        try
        {
            shiftJis = Encoding.GetEncoding("shift_jis");
        }
        catch
        {
            shiftJis = Encoding.GetEncoding(932);
        }

        var filePath = FileHelper.CreateTestFile("japanese_shiftjis.txt", JapaneseText, shiftJis);
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(JapaneseText);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_Utf16LE_WithEncodingDetection_ShouldReadJapaneseCorrectly()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("japanese_utf16le.txt", JapaneseText, Encoding.Unicode);
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(JapaneseText);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_MixedLanguages_WithEncodingDetection_ShouldReadCorrectly()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("mixed_utf8.txt", MixedText, new UTF8Encoding(false));
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(MixedText);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void Debug_ReadLine_StepByStep()
    {
        // Arrange
        var line1Content = $"Line 1: {JapaneseText}";
        var line2Content = $"Line 2: {ChineseText}";
        var line3Content = $"Line 3: {KoreanText}";
        var content = $"{line1Content}\n{line2Content}\n{line3Content}";
        var filePath = FileHelper.CreateTestFile("debug_multiline.txt", content, new UTF8Encoding(false));

        // Verify file was created correctly
        var fileContent = File.ReadAllText(filePath, new UTF8Encoding(false));
        fileContent.Should().Contain(line1Content);
        fileContent.Should().Contain(line2Content);
        fileContent.Should().Contain(line3Content);

        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act & Assert - Read line by line
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            var line1 = reader.ReadLine();
            line1.Should().NotBeNull("first line should not be null");
            line1.Should().Be(line1Content, $"first line should be '{line1Content}'");

            var line2 = reader.ReadLine();
            line2.Should().NotBeNull("second line should not be null");
            line2.Should().Be(line2Content, $"second line should be '{line2Content}'");

            var line3 = reader.ReadLine();
            line3.Should().NotBeNull("third line should not be null");
            line3.Should().Be(line3Content, $"third line should be '{line3Content}'");

            var line4 = reader.ReadLine();
            line4.Should().BeNull("fourth line should be null (EOF)");
        }

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_ReadLine_WithEncodingDetection_ShouldReadMultipleLinesCorrectly()
    {
        // Arrange
        var content = $"Line 1: {JapaneseText}\nLine 2: {ChineseText}\nLine 3: {KoreanText}";
        var filePath = FileHelper.CreateTestFile("multiline_utf8.txt", content, new UTF8Encoding(false));

        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string line1, line2, line3;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            line1 = reader.ReadLine();
            line2 = reader.ReadLine();
            line3 = reader.ReadLine();
        }

        // Assert
        line1.Should().Be($"Line 1: {JapaneseText}");
        line2.Should().Be($"Line 2: {ChineseText}");
        line3.Should().Be($"Line 3: {KoreanText}");

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_WithoutEncodingDetection_ShouldUseLegacyBehavior()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("legacy_test.txt", "Simple ASCII text", Encoding.UTF8);
        var options = new FilterReaderOptions { UseEncodingDetection = false };

        // Act & Assert - Should not throw, uses IFilter path
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            var result = reader.ReadToEnd();
            result.Should().NotBeNullOrEmpty();
        }

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_LogFile_WithEncodingDetection_ShouldWorkWithLogExtension()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("application.log", $"[INFO] {JapaneseText}", new UTF8Encoding(false));
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be($"[INFO] {JapaneseText}");

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FilterReader_CsvFile_WithEncodingDetection_ShouldWorkWithCsvExtension()
    {
        // Arrange
        var content = $"Name,Description\n製品,{JapaneseText}";
        var filePath = FileHelper.CreateTestFile("data.csv", content, new UTF8Encoding(false));

        // Verify the file was created correctly
        var verifyContent = File.ReadAllText(filePath, new UTF8Encoding(false));
        verifyContent.Should().Contain(JapaneseText, "file should be written with Japanese text");
        verifyContent.Should().Contain("製品", "file should be written with Japanese product name");

        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Debug output - will show in test output if test fails
        var debugInfo = $@"
            Test File: {filePath}
            Expected content: {content}
            Actual file content: {verifyContent}
            FilterReader result: {result}
            Result length: {result.Length}
            Expected JapaneseText ('{JapaneseText}'): {result.Contains(JapaneseText)}
            Expected '製品': {result.Contains("製品")}
            ";

        // Assert with better error messages
        result.Should().Contain(JapaneseText, $"FilterReader should read Japanese text.{debugInfo}");
        result.Should().Contain("製品", $"FilterReader should read Japanese product name.{debugInfo}");

        // Cleanup
        File.Delete(filePath);
    }

    [Theory]
    [InlineData("UTF-8", true)]
    [InlineData("UTF-8", false)]
    [InlineData("UTF-16")]
    [InlineData("Shift-JIS")]
    public void FilterReader_VariousEncodings_ShouldHandleCorrectly(string encodingName, bool useBom = true)
    {
        // Arrange
        Encoding encoding = encodingName switch
        {
            "UTF-8" => useBom ? Encoding.UTF8 : new UTF8Encoding(false),
            "UTF-16" => Encoding.Unicode,
            "Shift-JIS" => GetShiftJisEncoding(),
            _ => Encoding.UTF8
        };

        var filePath = FileHelper.CreateTestFile($"test_{encodingName.Replace("-", "")}.txt", JapaneseText, encoding);
        var options = new FilterReaderOptions { UseEncodingDetection = true };

        // Act
        string result;
        using (var reader = new FilterReader(filePath, filterReaderOptions: options))
        {
            result = reader.ReadToEnd();
        }

        // Assert
        result.Trim().Should().Be(JapaneseText, $"encoding {encodingName} should preserve Japanese text");

        // Cleanup
        File.Delete(filePath);
    }

    /// <summary>
    ///     Gets Shift-JIS encoding with fallback to code page 932
    /// </summary>
    private Encoding GetShiftJisEncoding()
    {
        try
        {
            return Encoding.GetEncoding("shift_jis");
        }
        catch
        {
            try
            {
                return Encoding.GetEncoding(932);
            }
            catch
            {
                // If Shift-JIS is not available, skip the test
                throw new SkipException("Shift-JIS encoding not available on this system");
            }
        }
    }
}

/// <summary>
///     Custom exception to skip tests when encoding is not available
/// </summary>
public class SkipException : Exception
{
    public SkipException(string message) : base(message)
    {
    }
}
