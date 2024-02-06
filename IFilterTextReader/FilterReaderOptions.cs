//
// FilterReaderOptions.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2023 Magic-Sessions. (www.magic-sessions.com)
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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace IFilterTextReader;

/// <summary>
///     A class with options that control the way the <see cref="FilterReader" /> processes files
/// </summary>
public class FilterReaderOptions
{
    #region Constructor
    /// <summary>
    ///     Creates this object and sets it's defaults properties
    /// </summary>
    public FilterReaderOptions()
    {
        DisableEmbeddedContent = false;
        IncludeProperties = false;
        ReadIntoMemory = false;
        ReaderTimeout = FilterReaderTimeout.NoTimeout;
        Timeout = -1;
        DoCleanUpCharacters = true;
        WordBreakSeparator = "-";
        ChunkTypeSeparator = " ";
    }
    #endregion

    #region Properties
    /// <summary>
    ///     When set to <c>true</c> the <see cref="NativeMethods.IFilter" />
    ///     doesn't read embedded content, e.g. an attachment inside an E-mail msg file. This parameter is default set to <c>false</c>
    /// </summary>
    public bool DisableEmbeddedContent { get; set; }

    /// <summary>
    ///     When set to <c>true</c> the metadata properties of
    ///     a document are also returned, e.g. the summary properties of a Word document. This parameter
    ///     is default set to <c>false</c>
    /// </summary>
    public bool IncludeProperties { get; set; }

    /// <summary>
    ///     When set to <c>true</c> the file to process is completely read
    ///     into memory first before the iFilters starts to read chunks, when set to <c>false</c> the iFilter reads
    ///     directly from the file and advances reading when the chunks are returned.
    ///     Default set to <c>false</c>
    /// </summary>
    public bool ReadIntoMemory { get; set; }

    /// <summary>
    ///     Can be used to timeout when parsing very large files, default set to <see cref="FilterReaderTimeout.NoTimeout" />
    /// </summary>
    public FilterReaderTimeout ReaderTimeout { get; set; }

    /// <summary>
    ///     The timeout in millisecond when the <see cref="FilterReaderTimeout" /> is set to a value other then <see cref="FilterReaderTimeout.NoTimeout" />
    /// </summary>
    /// <remarks>
    ///     This value is only
    ///     used when <see cref="FilterReaderTimeout" /> is set to <see cref="FilterReaderTimeout.TimeoutOnly" />
    ///     or <see cref="FilterReaderTimeout.TimeoutWithException" />
    /// </remarks>
    public int Timeout { get; set; }

    /// <summary>
    ///     Indicates when <c>true</c> (default) that certain characters should be translated to likely ASCII characters.
    /// </summary>
    public bool DoCleanUpCharacters { get; set; }

    /// <summary>
    ///     The separator that is used between word breaks
    /// </summary>
    public string WordBreakSeparator { get; set; }

    /// <summary>
    ///     The separator that is used between different chunk types
    /// </summary>
    public string ChunkTypeSeparator { get; set; }
    #endregion
}