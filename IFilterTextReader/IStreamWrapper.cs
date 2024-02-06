//
// IStreamWrapper.cs
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace IFilterTextReader;

/// <summary>
///     This class is used as a .NET wrapper around the <see cref="IStream" /> interface
/// </summary>
// ReSharper disable once InconsistentNaming
internal class IStreamWrapper : IStream
{
    #region Fields
    private readonly Stream _stream;
    #endregion

    #region Read
    /// <summary>
    ///     Reads data from the <see cref="IStream" />
    /// </summary>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    public void Read(byte[] pv, int cb, IntPtr pcbRead)
    {
        var bytesRead = _stream.Read(pv, 0, cb);

        if (pcbRead != IntPtr.Zero)
            Marshal.WriteInt32(pcbRead, bytesRead);
    }
    #endregion

    #region Write
    /// <summary>
    ///     Writes data to the <see cref="IStream" />
    /// </summary>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbWritten"></param>
    public void Write(byte[] pv, int cb, IntPtr pcbWritten)
    {
        _stream.Write(pv, 0, cb);

        if (pcbWritten != IntPtr.Zero)
            Marshal.WriteInt32(pcbWritten, cb);
    }
    #endregion

    #region Seek
    /// <summary>
    ///     Sets the position within the <see cref="IStream" />
    /// </summary>
    /// <param name="dlibMove"></param>
    /// <param name="dwOrigin"></param>
    /// <param name="plibNewPosition"></param>
    public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
    {
        var newPosition = _stream.Seek(dlibMove, (SeekOrigin)dwOrigin);

        if (plibNewPosition != IntPtr.Zero)
            Marshal.WriteInt64(plibNewPosition, newPosition);
    }
    #endregion

    #region Stat
    /// <summary>
    ///     Sets the length of the stream
    /// </summary>
    /// <param name="pstatstg"></param>
    /// <param name="grfStatFlag"></param>
    public void Stat(out STATSTG pstatstg, int grfStatFlag)
    {
        // IStreamWrapper wants the length
        var tempStatstg = new STATSTG { cbSize = _stream.Length };
        pstatstg = tempStatstg;
    }
    #endregion

    #region Constructor
    /// <summary>
    ///     Creates a <see cref="Stream" /> wrapper around an <see cref="IStream" />
    /// </summary>
    /// <param name="stream"></param>
    public IStreamWrapper(Stream stream)
    {
        // ReSharper disable once LocalizableElement
        _stream = stream ?? throw new ArgumentNullException(nameof(stream), "Can't wrap null stream.");
    }
    #endregion

    #region Not Implemented
    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <param name="ppstm"></param>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void Clone(out IStream ppstm)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Commit changes to the stream.
    /// </summary>
    /// <param name="grfCommitFlags"></param>
    public void Commit(int grfCommitFlags)
    {
        _stream.Flush();
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <param name="pstm"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <param name="pcbWritten"></param>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <param name="libOffset"></param>
    /// <param name="dwLockType"></param>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void LockRegion(long libOffset, int dwLockType)
    {
        LockRegion(libOffset, 0, dwLockType);
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <param name="libOffset"></param>
    /// <param name="cb"></param>
    /// <param name="dwLockType"></param>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void LockRegion(long libOffset, long cb, int dwLockType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void Revert()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void SetSize()
    {
        SetSize(0);
    }

    /// <summary>
    ///     Set the size of the stream.
    /// </summary>
    public void SetSize(long libNewSize)
    {
        _stream.SetLength(libNewSize);
    }

    /// <summary>
    ///     This function is not implemented and will throw an NotImplementedException
    /// </summary>
    /// <param name="libOffset"></param>
    /// <param name="cb"></param>
    /// <param name="dwLockType"></param>
    /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
    public void UnlockRegion(long libOffset, long cb, int dwLockType)
    {
        throw new NotImplementedException();
    }
    #endregion
}