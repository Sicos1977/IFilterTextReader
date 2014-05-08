using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Email2Storage.Modules.Readers.TextReader
{
    /// <summary>
    /// This class is used as a .NET wrapper around the <see cref="IStream"/> interface
    /// </summary>
    internal class StreamWrapper : IStream
    {
        #region Fields
        private readonly Stream _stream;
        #endregion

        #region Constructor
        public StreamWrapper(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "Can't wrap null stream.");

            _stream = stream;
        }
        #endregion

        #region Read
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            Marshal.WriteInt32(pcbRead, _stream.Read(pv, 0, cb));
        }
        #endregion

        #region Write
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            int written = Marshal.ReadInt32(pcbWritten);
            _stream.Write(pv, 0, written);
        }
        #endregion

        #region Seek
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            //Marshal.WriteInt32(plibNewPosition, (int)stream.Seek(dlibMove, (SeekOrigin)dwOrigin));
            _stream.Seek(dlibMove, (SeekOrigin)(dwOrigin));
        }
        #endregion

        #region Not Implemented
        public void Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        public void Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new NotImplementedException();
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Revert()
        {
            throw new NotImplementedException();
        }

        public void SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }
        
        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Stat
        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            //IStreamWrapper wants the length
            var tempStatstg = new System.Runtime.InteropServices.ComTypes.STATSTG {cbSize = _stream.Length};
            pstatstg = tempStatstg;
        }
        #endregion
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// This class acts as a wrapper around the <see cref="IStream"/> interface
    /// </summary>
    internal class IStreamWrapper : Stream
    {
        #region Fields
        /// <summary>
        /// The <see cref="IStream"/> interface
        /// </summary>
        IStream _stream;
        #endregion

        #region Constructor and Destructor
        public IStreamWrapper(IStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            
            _stream = stream;
        }

        ~IStreamWrapper()
        {
            Close();
        }
        #endregion

        #region Read
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
                throw new NotSupportedException("Only offset 0 is supported");

            if (buffer.Length == 0)
                throw new ArgumentException("The buffer is empty");

            if (buffer.Length < count)
                throw new NotSupportedException("Buffer is not large enough");

            var bytesRead = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
            
            try
            {
                _stream.Read(buffer, count, bytesRead);
                return Marshal.ReadInt32(bytesRead);
            }
            finally
            {
                Marshal.FreeCoTaskMem(bytesRead);
            }
        }
        #endregion

        #region Write
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
                throw new NotSupportedException("Only Offset is supported");

            _stream.Write(buffer, count, IntPtr.Zero);
        }
        #endregion

        #region Seek
        public override long Seek(long offset, SeekOrigin origin)
        {
            var address = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
            try
            {
                _stream.Seek(offset, (int)origin, address);
                return Marshal.ReadInt32(address);
            }
            finally
            {
                Marshal.FreeCoTaskMem(address);
            }
        }
        #endregion

        #region Length
        public override long Length
        {
            get
            {
                System.Runtime.InteropServices.ComTypes.STATSTG statstg;
                _stream.Stat(out statstg, 1 /* STATSFLAG_NONAME*/ );
                return statstg.cbSize;
            }
        }
        #endregion

        #region Position
        public override long Position
        {
            get { return Seek(0, SeekOrigin.Current); }
            set { Seek(value, SeekOrigin.Begin); }
        }
        #endregion

        #region SetLength
        public override void SetLength(long value)
        {
            _stream.SetSize(value);
        }
        #endregion

        #region Close
        public override void Close()
        {
            _stream.Commit(0);
            _stream = null;
        }
        #endregion

        #region Flush
        public override void Flush()
        {
            _stream.Commit(0);
        }
        #endregion

        #region CanRead
        public override bool CanRead
        {
            get { return true; }
        }
        #endregion

        #region CanWrite
        public override bool CanWrite
        {
            get { return true; }
        }
        #endregion

        #region CanSeek
        public override bool CanSeek
        {
            get { return true; }
        }
        #endregion
    }
}