using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Email2Storage.Modules.Readers.IFilterTextReader
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
}