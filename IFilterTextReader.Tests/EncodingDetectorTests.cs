//
// EncodingDetectorTests.cs
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
using IFilterTextReader.Helpers;
using IFilterTextReader.Tests.Helpers;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace IFilterTextReader.Tests;

/// <summary>
///     Tests for encoding detection functionality
/// </summary>
public class EncodingDetectorTests
{
    private const string TestFilesPath = "TestFiles";

    [Fact]
    public void DetectEncoding_Utf8WithBom_ShouldReturnUtf8()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("utf8_bom.txt", "テスト test 测试", Encoding.UTF8);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        encoding.Should().BeOfType<UTF8Encoding>();
        encoding.GetPreamble().Should().HaveCount(3); // UTF-8 BOM is 3 bytes

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_Utf8WithoutBom_ShouldReturnUtf8()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("utf8_nobom.txt", "テスト test 测试", new UTF8Encoding(false));

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        encoding.Should().BeOfType<UTF8Encoding>();
        encoding.GetPreamble().Should().BeEmpty(); // No BOM

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_Utf16LE_ShouldReturnUnicode()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("utf16le.txt", "テスト test 测试", Encoding.Unicode);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        encoding.Should().BeOfType<UnicodeEncoding>();
        encoding.GetPreamble().Should().Equal(0xFF, 0xFE); // UTF-16 LE BOM

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_Utf16BE_ShouldReturnBigEndianUnicode()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("utf16be.txt", "テスト test 测试", Encoding.BigEndianUnicode);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        encoding.Should().BeOfType<UnicodeEncoding>();
        encoding.GetPreamble().Should().Equal(0xFE, 0xFF); // UTF-16 BE BOM

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_ShiftJis_ShouldReturnShiftJis()
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

        var filePath = FileHelper.CreateTestFile("shiftjis.txt", "テスト", shiftJis);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        // Should detect as Shift-JIS (code page 932 or shift_jis)
        (encoding.CodePage == 932 || encoding.WebName == "shift_jis").Should().BeTrue();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_AsciiOnly_ShouldReturnUtf8()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("ascii.txt", "Hello World! This is a test.", Encoding.ASCII);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        // ASCII files without multi-byte chars should default to UTF-8
        encoding.Should().BeOfType<UTF8Encoding>();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_EmptyFile_ShouldReturnUtf8()
    {
        // Arrange
        var filePath = FileHelper.CreateTestFile("empty.txt", "", Encoding.UTF8);

        // Act
        var encoding = EncodingDetector.DetectEncoding(filePath);

        // Assert
        encoding.Should().BeOfType<UTF8Encoding>();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void DetectEncoding_FromStream_ShouldPreserveStreamPosition()
    {
        // Arrange
        var content = "テスト test";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);
        stream.Position = 5; // Set position somewhere in the middle

        // Act
        var encoding = EncodingDetector.DetectEncoding(stream);

        // Assert
        encoding.Should().BeOfType<UTF8Encoding>();
        stream.Position.Should().Be(5); // Position should be restored
    }

    [Fact]
    public void DetectEncoding_EmptyFile_ReturnsUtf8WithoutBom()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            File.WriteAllBytes(file, Array.Empty<byte>());
            var enc = EncodingDetector.DetectEncoding(file);
    
            enc.Should().NotBeNull();
            enc.WebName.Should().Be("utf-8");
            enc.GetPreamble().Length.Should().Be(0);
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
    
    [Fact]
    public void DetectEncoding_PreservesStreamPosition()
    {
        var bytes = Encoding.UTF8.GetBytes("prefixテストsuffix");
        using var ms = new MemoryStream(bytes);
        ms.Position = 5;
        var enc = EncodingDetector.DetectEncoding(ms);
        ms.Position.Should().Be(5);
        enc.Should().NotBeNull();
        enc.WebName.Should().Be("utf-8");
        enc.GetPreamble().Length.Should().Be(0);
    }
    
    [Fact]
    public void DetectEncoding_ShiftJis_ReturnsCodePage932_WhenAvailable()
    {
        Encoding shiftJis;
        try
        {
            shiftJis = Encoding.GetEncoding("shift_jis");
        }
        catch
        {
            // Skip if Shift-JIS not available on platform - treat as pass
            return;
        }
    
        var text = "テスト";
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            File.WriteAllBytes(file, shiftJis.GetBytes(text));
    
            var enc = EncodingDetector.DetectEncoding(file);
    
            enc.Should().NotBeNull();
            // Accept either the named encoding or code page 932
            (enc.CodePage == shiftJis.CodePage || enc.CodePage == 932).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
    
    [Fact]
    public void DetectEncoding_InvalidUtf8_FallsBackToUtf8WithoutBom()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
        try
        {
            // invalid/garbled bytes
            File.WriteAllBytes(file, new byte[] { 0xFF, 0xFE, 0x00, 0x54 });
            var enc = EncodingDetector.DetectEncoding(file);
    
            enc.Should().NotBeNull();
            enc.WebName.Should().Be("utf-8");
            enc.GetPreamble().Length.Should().Be(0);
        }
        finally
        {
            if (File.Exists(file)) File.Delete(file);
        }
    }
}
