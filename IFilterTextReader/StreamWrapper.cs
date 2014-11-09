using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace IFilterTextReader
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
        /// <summary>
        /// Creates a <see cref="Stream"/> wrapper around an <see cref="IStream"/>
        /// </summary>
        /// <param name="stream"></param>
        public StreamWrapper(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "Can't wrap null stream.");

            _stream = stream;
        }
        #endregion

        #region Read
        /// <summary>
        /// Reads data from the <see cref="IStream"/>
        /// </summary>
        /// <param name="pv"></param>
        /// <param name="cb"></param>
        /// <param name="pcbRead"></param>
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            Marshal.WriteInt32(pcbRead, _stream.Read(pv, 0, cb));
        }
        #endregion

        #region Write
        /// <summary>
        /// Writes data to the <see cref="IStream"/>
        /// </summary>
        /// <param name="pv"></param>
        /// <param name="cb"></param>
        /// <param name="pcbWritten"></param>
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            var written = Marshal.ReadInt32(pcbWritten);
            _stream.Write(pv, 0, written);
        }
        #endregion

        #region Seek
        /// <summary>
        /// Sets the position within the <see cref="IStream"/>
        /// </summary>
        /// <param name="dlibMove"></param>
        /// <param name="dwOrigin"></param>
        /// <param name="plibNewPosition"></param>
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            _stream.Seek(dlibMove, (SeekOrigin)(dwOrigin));
        }
        #endregion

        #region Not Implemented
        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <param name="ppstm"></param>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <param name="grfCommitFlags"></param>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
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
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <param name="libOffset"></param>
        /// <param name="dwLockType"></param>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void LockRegion(long libOffset, int dwLockType)
        {
            LockRegion(libOffset, 0, dwLockType);
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
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
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void Revert()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void SetSize()
        {
            SetSize(0);
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
        /// </summary>
        /// <exception cref="NotImplementedException">This exception will ALWAYS be thrown</exception>
        public void SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is not implemented and will throw an NotImplementedException
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

        #region Stat
        /// <summary>
        /// Sets the length of the stream
        /// </summary>
        /// <param name="pstatstg"></param>
        /// <param name="grfStatFlag"></param>
        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            //IStreamWrapper wants the length
            var tempStatstg = new System.Runtime.InteropServices.ComTypes.STATSTG {cbSize = _stream.Length};
            pstatstg = tempStatstg;
        }
        #endregion
    }
}