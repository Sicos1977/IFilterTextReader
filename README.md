IFilterTextReader
=================

A C# TextReader that gets text from different file formats through the IFilter interface

Installing via NuGet
====================

The easiest way to install IFilterTextReader is via NuGet.

In Visual Studio's Package Manager Console, simply enter the following command:

    Install-Package IFilterTextReader

Known Limitations
====================

Japanese and Multi-byte Character Encoding Issues

**Problem**: When extracting text from files encoded in UTF-8 or Shift-JIS, Japanese 
characters may appear as mathematical symbols or garbled text.

**Root Cause**: This is a limitation of the underlying Windows IFilter implementations, 
which may incorrectly detect the encoding of plain text files. UTF-16 encoded files 
work correctly because the BOM (Byte Order Mark) helps IFilter identify the encoding.

**Workaround**: 
- Use UTF-16 (with BOM) encoding for text files containing Japanese characters
- For existing UTF-8/Shift-JIS files, consider enabling the `UseEncodingDetection` 
  option in `FilterReaderOptions` (if available)

**Affected Encodings**:
- UTF-8 (without BOM)
- Shift-JIS (SJIS)
- Other multi-byte encodings

**Working Encodings**:
- UTF-16 LE (with BOM)
- UTF-16 BE (with BOM)

## License Information

IFilterTextReader is Copyright (C)2013-2024 Kees van Spelde (Magic-Sessions) and is licensed under the MIT license:

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.

Core Team
=========
    Sicos1977 (Kees van Spelde)
