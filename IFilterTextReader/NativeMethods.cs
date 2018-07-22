using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

/*
   Copyright 2013-2018 Kees van Spelde

   Licensed under The Code Project Open License (CPOL) 1.02;
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.codeproject.com/info/cpol10.aspx

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace IFilterTextReader
{
    /// <summary>
    /// Contains all the used native methods
    /// </summary>
    internal static class NativeMethods
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedField.Compiler
        // ReSharper disable UnassignedField.Compiler
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable UnusedMember.Global

        /// <summary>
        /// DllGetClassObject fuction pointer signature
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="interfaceId"></param>
        /// <param name="ppunk"></param>
        /// <returns></returns>
        internal delegate int DllGetClassObject(ref Guid classId, 
                                              ref Guid interfaceId, 
                                              [Out, MarshalAs(UnmanagedType.Interface)] 
                                              out object ppunk);

        #region Enum IFILTER_FLAGS
        [Flags]
        internal enum IFILTER_FLAGS
        {
            /// <summary>
            /// The caller should use the IPropertySetStorage and IPropertyStorage interfaces to locate additional properties. 
            /// When this flag is set, properties available through COM enumerators should not be returned from IFilter. 
            /// </summary>
            IFILTER_FLAGS_OLE_PROPERTIES = 1
        }
        #endregion

        #region Enum IFILTER_INIT
        /// <summary>
        /// Flags controlling the operation of the <see cref="IFilter"/>
        /// </summary>
        [Flags]
        internal enum IFILTER_INIT
        {
            NONE = 0,

            /// <summary>
            /// Paragraph breaks should be marked with the Unicode PARAGRAPH SEPARATOR (0x2029)
            /// </summary>
            CANON_PARAGRAPHS = 1,

            /// <summary>
            /// Soft returns, such as the newline character in Microsoft Word, should be replaced by hard returns LINE SEPARATOR (0x2028). Existing hard
            /// returns can be doubled. A carriage return (0x000D), line feed (0x000A), or the carriage return and line feed in combination should be considered
            /// a hard return. The intent is to enable pattern-expression matches that match against observed line breaks. 
            /// </summary>
            HARD_LINE_BREAKS = 2,

            /// <summary>
            /// Various word-processing programs have forms of hyphens that are not represented in the host character set, such as optional hyphens
            /// (appearing only at the end of a line) and nonbreaking hyphens. This flag indicates that optional hyphens are to be converted to nulls, and
            /// non-breaking hyphens are to be converted to normal hyphens (0x2010), or HYPHEN-MINUSES (0x002D). 
            /// </summary>
            CANON_HYPHENS = 4,

            /// <summary>
            /// Just as the CANON_HYPHENS flag standardizes hyphens, this one standardizes spaces. All special space characters, such as
            /// nonbreaking spaces, are converted to the standard space character (0x0020). 
            /// </summary>
            CANON_SPACES = 8,

            /// <summary>
            /// Indicates that the client wants text split into chunks representing public value-type properties. 
            /// </summary>
            APPLY_INDEX_ATTRIBUTES = 16,

            /// <summary>
            /// Indicates that the client wants text split into chunks representing properties determined during the indexing process. 
            /// </summary>
            APPLY_CRAWL_ATTRIBUTES = 256,

            /// <summary>
            /// Any properties not covered by the APPLY_INDEX_ATTRIBUTES and APPLY_CRAWL_ATTRIBUTES flags should be emitted. 
            /// </summary>
            APPLY_OTHER_ATTRIBUTES = 32,

            /// <summary>
            /// Optimizes IFilter for indexing because the client calls the IFilter::Init method only once and does not call IFilter::BindRegion.
            /// This eliminates the possibility of accessing a chunk both before and after accessing another chunk. 
            /// </summary>
            INDEXING_ONLY = 64,

            /// <summary>
            /// The text extraction process must recursively search all linked objects within the document. If a link is unavailable, the
            /// IFilter::GetChunk call that would have obtained the first chunk of the link should return FILTER_E_LINK_UNAVAILABLE. 
            /// </summary>
            SEARCH_LINKS = 128,

            /// <summary>
            /// The content indexing process can return property values set by the filter. 
            /// </summary>
            FILTER_OWNED_VALUE_OK = 512,

            /// <summary>
            /// Text should be broken in chunks more aggressively than normal. 
            /// </summary>
            FILTER_AGGRESSIVE_BREAK	= 1024,

            /// <summary>
            /// The IFilter should not return chunks from embedded content. 
            /// </summary>
	        DISABLE_EMBEDDED = 2048,

            /// <summary>
            /// The IFilter should emit formatting info.
            /// </summary>
	        EMIT_FORMATTING	= 4096
        }
        #endregion
        
        #region Enum IFilterReturnCode
        /// <summary>
        /// The return codes used by the <see cref="IFilter"/>
        /// </summary>
        internal enum IFilterReturnCode : uint
        {
            /// <summary>
            /// Success
            /// </summary>
            S_OK = 0,

            /// <summary>
            /// The function was denied access to the filter file. 
            /// </summary>
            E_ACCESSDENIED = 0x80070005,

            /// <summary>
            /// The function encountered an invalid handle, probably due to a low-memory situation. 
            /// </summary>
            E_HANDLE = 0x80070006,

            /// <summary>
            /// The function received an invalid parameter.
            /// </summary>
            E_INVALIDARG = 0x80070057,

            /// <summary>
            /// Out of memory
            /// </summary>
            E_OUTOFMEMORY = 0x8007000E,

            /// <summary>
            /// Not implemented
            /// </summary>
            E_NOTIMPL = 0x80004001,

            /// <summary>
            /// Unknown error
            /// </summary>
            E_FAIL = 0x80000008,

            /// <summary>
            /// File not filtered due to password protection
            /// </summary>
            FILTER_E_PASSWORD = 0x8004170B,

            /// <summary>
            /// The document format is not recognised by the filter
            /// </summary>
            FILTER_E_UNKNOWNFORMAT = 0x8004170C,

            /// <summary>
            /// No text in current chunk
            /// </summary>
            FILTER_E_NO_TEXT = 0x80041705,

            /// <summary>
            /// No more chunks of text available in object
            /// </summary>
            FILTER_E_END_OF_CHUNKS = 0x80041700,

            /// <summary>
            /// No more text available in chunk
            /// </summary>
            FILTER_E_NO_MORE_TEXT = 0x80041701,

            /// <summary>
            /// The file contains no values
            /// </summary>
            FILTER_E_NO_VALUES = 0x80041706,

            /// <summary>
            /// No more property values available in chunk
            /// </summary>
            FILTER_E_NO_MORE_VALUES = 0x80041702,

            /// <summary>
            /// Unable to access object
            /// </summary>
            FILTER_E_ACCESS = 0x80041703,

            /// <summary>
            /// Moniker doesn't cover entire region
            /// </summary>
            FILTER_W_MONIKER_CLIPPED = 0x00041704,

            /// <summary>
            /// Unable to bind IFilter for embedded object
            /// </summary>
            FILTER_E_EMBEDDING_UNAVAILABLE = 0x80041707,

            /// <summary>
            /// Unable to bind IFilter for linked object
            /// </summary>
            FILTER_E_LINK_UNAVAILABLE = 0x80041708,

            /// <summary>
            /// This is the last text in the current chunk
            /// </summary>
            FILTER_S_LAST_TEXT = 0x00041709,

            /// <summary>
            /// This is the last value in the current chunk
            /// </summary>
            FILTER_S_LAST_VALUES = 0x0004170A,
            
            /// <summary>
            /// The document was too large to filter in its entirety. 
            /// Portions of the document were not emitted.
            /// </summary>
            FILTER_E_PARTIALLY_FILTERED = 0x8004173E,

            /// <summary>
            /// File is too large to filter.
            /// </summary>
            FILTER_E_TOO_BIG = 0x80041730
        }
        #endregion

        #region Enum CHUNK_BREAKTYPE
        /// <summary>
        /// Enumerates the different breaking types that occur between chunks of text read out by the <see cref="IFilter"/>.
        /// </summary>
        internal enum CHUNK_BREAKTYPE
        {
            /// <summary>
            /// No break is placed between the current chunk and the previous chunk. The chunks are glued together. 
            /// </summary>
            CHUNK_NO_BREAK = 0,

            /// <summary>
            /// A word break is placed between this chunk and the previous chunk that had the same attribute. 
            /// Use of CHUNK_EOW should be minimized because the choice of word breaks is language-dependent, 
            /// so determining word breaks is best left to the search engine. /// </summary>
            CHUNK_EOW = 1,

            /// <summary>
            /// A sentence break is placed between this chunk and the previous chunk that had the same attribute. 
            /// </summary>
            CHUNK_EOS = 2,

            /// <summary>
            /// A paragraph break is placed between this chunk and the previous chunk that had the same attribute.
            /// </summary>     
            CHUNK_EOP = 3,

            /// <summary>
            /// A chapter break is placed between this chunk and the previous chunk that had the same attribute. 
            /// </summary>
            CHUNK_EOC = 4
        }
        #endregion

        #region Enum CHUNKSTATE
        /// <summary>
        /// The state of the chunck that has been read bij the <see cref="IFilter"/>
        /// </summary>
        [Flags]
        internal enum CHUNKSTATE
        {
            /// <summary>
            /// The current chunk is a text-type property.
            /// </summary>
            CHUNK_TEXT = 0x1,

            /// <summary>
            /// The current chunk is a value-type property. 
            /// </summary>
            CHUNK_VALUE = 0x2,

            /// <summary>
            /// Reserved
            /// </summary>
            CHUNK_FILTER_OWNED_VALUE = 0x4
        }
        #endregion

        #region Enum PROPSPECKIND
        /// <summary>
        /// Types of properties returned by the <see cref="IFilter"/>
        /// </summary>
        internal enum PROPSPECKIND
        {
            /// <summary>
            /// The property's name is a string
            /// </summary>
            PRSPEC_LPWSTR = 0,

            /// <summary>
            /// The property's name is a well known property id
            /// </summary>
            PRSPEC_PROPID = 1
        }
        #endregion

        #region Enum JobObjectInfoType
        internal enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11
        }
        #endregion

        #region Struct STAT_CHUNK
#pragma warning disable 0649
        internal struct STAT_CHUNK
        {
            /// <summary>
            /// The chunk identifier. Chunk identifiers must be unique for the current instance of the IFilter interface. 
            /// Chunk identifiers must be in ascending order. The order in which chunks are numbered should correspond to the order in which they appear
            /// in the source document. Some search engines can take advantage of the proximity of chunks of various properties. If so, the order in which
            /// chunks with different properties are emitted will be important to the search engine. 
            /// </summary>
            public int idChunk;

            /// <summary>
            /// The type of break that separates the previous chunk from the current chunk. Values are from the CHUNK_BREAKTYPE enumeration. 
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public CHUNK_BREAKTYPE breakType;

            /// <summary>
            /// Flags indicate whether this chunk contains a text-type or a value-type property. 
            /// Flag values are taken from the CHUNKSTATE enumeration. If the CHUNK_TEXT flag is set, IFilter::GetText should be used to retrieve the 
            /// contents of the chunk as a series of words. If the CHUNK_VALUE flag is set, IFilter::GetValue should be used to retrieve the value and 
            /// treat it as a single property value. If the filter dictates that the same content be treated as both text and as a value, the chunk
            /// should be emitted twice in two different chunks, each with one flag set. 
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public CHUNKSTATE flags;

            /// <summary>
            /// The language and sublanguage associated with a chunk of text. Chunk locale is used by document indexers to perform proper word breaking 
            /// of text. If the chunk is neither text-type nor a value-type with data type VT_LPWSTR, VT_LPSTR or VT_BSTR, this field is ignored. 
            /// </summary>
            public int locale;

            /// <summary>
            /// The property to be applied to the chunk. If a filter requires that the same text have more than one property, it needs to emit the 
            /// text once for each property in separate chunks. 
            /// </summary>
            public FULLPROPSPEC attribute;

            /// <summary>
            /// The ID of the source of a chunk. The value of the idChunkSource member depends on the nature of the chunk: 
            /// If the chunk is a text-type property, the value of the idChunkSource member must be the same as the value of the idChunk member. 
            /// If the chunk is an public value-type property derived from textual content, the value of the idChunkSource member is the chunk ID for the
            /// text-type chunk from which it is derived. If the filter attributes specify to return only public value-type properties, there is no
            /// content chunk from which to derive the current public value-type property. In this case, the value of the idChunkSource member must be 
            /// set to zero, which is an invalid chunk. 
            /// </summary>
            public int idChunkSource;

            /// <summary>
            /// The offset from which the source text for a derived chunk starts in the source chunk. 
            /// </summary>
            public int cwcStartSource;

            /// <summary>
            /// The length in characters of the source text from which the current chunk was derived. 
            /// A zero value signifies character-by-character correspondence between the source text and 
            /// the derived text. A nonzero value means that no such direct correspondence exists
            /// </summary>
            public int cwcLenSource;
        }
#pragma warning restore 0649
        #endregion

        #region Struct FILTERREGION
        [StructLayout(LayoutKind.Sequential)]
        internal struct FILTERREGION
        {
            public int idChunk;
            public int cwcStart;
            public int cwcExtent;
        }
        #endregion

        #region Struct FULLPROPSPEC
        [StructLayout(LayoutKind.Sequential)]
        internal struct FULLPROPSPEC
        {
            public Guid guidPropSet;
            public PROPSPEC psProperty;
        }
        #endregion

        #region Struct PROPSPEC
        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct PROPSPEC
        {
            [MarshalAs(UnmanagedType.U4)] 
            public PROPSPECKIND ulKind; // PRSPEC_LPWSTR or PRSPEC_PROPID
            public IntPtr data;
        }
        #endregion

        #region Struct PROPVARIANT
        /// <summary>
        /// Represents the OLE struct PROPVARIANT.
        /// </summary>
        /// <remarks>
        /// Must call Clear when finished to avoid memory leaks. If you get the value of
        /// a VT_UNKNOWN prop, an implicit AddRef is called, thus your reference will
        /// be active even after the PropVariant struct is cleared.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROPVARIANT
        {
            #region Struct fields
            // The layout of these elements needs to be maintained.
            //
            // NOTE: We could use LayoutKind.Explicit, but we want
            //       to maintain that the IntPtr may be 8 bytes on
            //       64-bit architectures, so we'll let the CLR keep
            //       us aligned.
            //
            // NOTE: In order to allow x64 compat, we need to allow for
            //       expansion of the IntPtr. However, the BLOB struct
            //       uses a 4-byte int, followed by an IntPtr, so
            //       although the p field catches most pointer values,
            //       we need an additional 4-bytes to get the BLOB
            //       pointer. The p2 field provides this, as well as
            //       the last 4-bytes of an 8-byte value on 32-bit
            //       architectures.

            // This is actually a VarEnum value, but the VarEnum type
            // shifts the layout of the struct by 4 bytes instead of the
            // expected 2.
            private ushort vt;
            private ushort wReserved1;
            private ushort wReserved2;
            private ushort wReserved3;
            private IntPtr p;
            private int p2;
            #endregion // struct fields

            #region Union members
            private sbyte cVal // CHAR cVal;
            {
                get { return (sbyte) GetDataBytes()[0]; }
            }

            private byte bVal // UCHAR bVal;
            {
                get { return GetDataBytes()[0]; }
            }

            private short iVal // SHORT iVal;
            {
                get { return BitConverter.ToInt16(GetDataBytes(), 0); }
            }

            private ushort uiVal // USHORT uiVal;
            {
                get { return BitConverter.ToUInt16(GetDataBytes(), 0); }
            }

            private int lVal // LONG lVal;
            {
                get { return BitConverter.ToInt32(GetDataBytes(), 0); }
            }

            private uint ulVal // ULONG ulVal;
            {
                get { return BitConverter.ToUInt32(GetDataBytes(), 0); }
            }

            private long hVal // LARGE_INTEGER hVal;
            {
                get { return BitConverter.ToInt64(GetDataBytes(), 0); }
            }

            private ulong uhVal // ULARGE_INTEGER uhVal;
            {
                get { return BitConverter.ToUInt64(GetDataBytes(), 0); }
            }

            private float fltVal // FLOAT fltVal;
            {
                get { return BitConverter.ToSingle(GetDataBytes(), 0); }
            }

            private double dblVal // DOUBLE dblVal;
            {
                get { return BitConverter.ToDouble(GetDataBytes(), 0); }
            }

            private bool boolVal // VARIANT_BOOL boolVal;
            {
                get { return (iVal != 0); }
            }

            private int scode // SCODE scode;
            {
                get { return lVal; }
            }

            private decimal cyVal // CY cyVal;
            {
                get { return decimal.FromOACurrency(hVal); }
            }

            private DateTime date // DATE date;
            {
                get { return DateTime.FromOADate(dblVal); }
            }
            #endregion // union members

            #region Helper methods
            /// <summary>
            /// Gets a byte array containing the data bits of the struct.
            /// </summary>
            /// <returns>A byte array that is the combined size of the data bits.</returns>
            private byte[] GetDataBytes()
            {
                var ret = new byte[IntPtr.Size + sizeof (int)];
                switch (IntPtr.Size)
                {
                    case 4:
                        BitConverter.GetBytes(p.ToInt32()).CopyTo(ret, 0);
                        break;
                    case 8:
                        BitConverter.GetBytes(p.ToInt64()).CopyTo(ret, 0);
                        break;
                }
                BitConverter.GetBytes(p2).CopyTo(ret, IntPtr.Size);
                return ret;
            }

            /// <summary>
            /// Called to properly clean up the memory referenced by a PropVariant instance.
            /// </summary>
            [DllImport("ole32.dll")]
            private static extern int PropVariantClear(ref PROPVARIANT pvar);

            /// <summary>
            /// Called to clear the PropVariant's referenced and local memory.
            /// </summary>
            /// <remarks>
            /// You must call Clear to avoid memory leaks.
            /// </remarks>
            public void Clear()
            {
                // Can't pass "this" by ref, so make a copy to call PropVariantClear with
                var var = this;
                PropVariantClear(ref var);

                // Since we couldn't pass "this" by ref, we need to clear the member fields manually
                // NOTE: PropVariantClear already freed heap data for us, so we are just setting
                //       our references to null.
                vt = (ushort) VarEnum.VT_EMPTY;
                wReserved1 = wReserved2 = wReserved3 = 0;
                p = IntPtr.Zero;
                p2 = 0;
            }

            /// <summary>
            /// Gets the variant type.
            /// </summary>
            public VarEnum Type
            {
                get { return (VarEnum) vt; }
            }

            /// <summary>
            /// Gets the variant value.
            /// </summary>
            public object Value
            {
                get
                {
                    // TODO: Add support for reference types (ie. VT_REF | VT_I1)
                    // TODO: Add support for safe arrays

                    switch ((VarEnum) vt)
                    {
                        case VarEnum.VT_I1:
                            return cVal;

                        case VarEnum.VT_UI1:
                            return bVal;

                        case VarEnum.VT_I2:
                            return iVal;

                        case VarEnum.VT_UI2:
                            return uiVal;

                        case VarEnum.VT_I4:
                        case VarEnum.VT_INT:
                            return lVal;

                        case VarEnum.VT_UI4:
                        case VarEnum.VT_UINT:
                            return ulVal;

                        case VarEnum.VT_I8:
                            return hVal;

                        case VarEnum.VT_UI8:
                            return uhVal;

                        case VarEnum.VT_R4:
                            return fltVal;

                        case VarEnum.VT_R8:
                            return dblVal;

                        case VarEnum.VT_BOOL:
                            return boolVal;

                        case VarEnum.VT_ERROR:
                            return scode;

                        case VarEnum.VT_CY:
                            return cyVal;

                        case VarEnum.VT_DATE:
                            return date;

                        case VarEnum.VT_FILETIME:
                            return DateTime.FromFileTime(hVal);

                        case VarEnum.VT_BSTR:
                            return Marshal.PtrToStringBSTR(p);

                        case VarEnum.VT_BLOB:
                            var blobData = new byte[lVal];
                            IntPtr pBlobData;
                            switch (IntPtr.Size)
                            {
                                case 4:
                                    pBlobData = new IntPtr(p2);
                                    break;
                                case 8:
                                    pBlobData = new IntPtr(BitConverter.ToInt64(GetDataBytes(), sizeof (int)));
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                            Marshal.Copy(pBlobData, blobData, 0, lVal);
                            return blobData;

                        case VarEnum.VT_LPSTR:
                            return Marshal.PtrToStringAnsi(p);

                        case VarEnum.VT_LPWSTR:
                            return Marshal.PtrToStringUni(p);

                        case VarEnum.VT_UNKNOWN:
                            return Marshal.GetObjectForIUnknown(p);

                        case VarEnum.VT_DISPATCH:
                            return p;

                        default:
                            return string.Empty;
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Struct PROPERTYKEY
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct PROPERTYKEY
        {
            public Guid fmtid;
            public long pid;
        }
        #endregion

        #region Struct IO_COUNTERS
        [StructLayout(LayoutKind.Sequential)]
        internal struct IO_COUNTERS
        {
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
        }
        #endregion

        #region Struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public Int64 PerProcessUserTimeLimit;
            public Int64 PerJobUserTimeLimit;
            public UInt32 LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            public UIntPtr Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
        }
        #endregion

        #region Struct SECURITY_ATTRIBUTES
        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            public UInt32 nLength;
            public IntPtr lpSecurityDescriptor;
            public Int32 bInheritHandle;
        }
        #endregion

        #region Struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        [StructLayout(LayoutKind.Sequential)]
        internal struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }
        #endregion

        #region Interface IFilter
        [ComImport, Guid("89BCB740-6119-101A-BCB7-00DD010655AF")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFilter
        {
            /// <summary>
            /// The IFilter::Init method initializes a filtering session.
            /// </summary>
            [PreserveSig]
            IFilterReturnCode Init(
                // [in] Flag settings from the IFILTER_INIT enumeration for controlling text standardization, property output, embedding
                // scope, and IFilter access patterns. 
                [MarshalAs(UnmanagedType.U4)]
                IFILTER_INIT grfFlags,

                // [in] The size of the attributes array. When nonzero, cAttributes takes precedence over attributes specified in grfFlags. 
                // If no attribute flags are specified and cAttributes is zero, the default is given by the  PSGUID_STORAGE storage property 
                // set, which contains the date and time of the last write to the file, size, and so on; and by the PID_STG_CONTENTS 
                // 'contents' property, which maps to the main contents of the file. 
                // For more information about properties and property sets, see Property Sets. 
                int cAttributes,

                // [in] Array of pointers to FULLPROPSPEC structures for the requested properties. When cAttributes is nonzero, only 
                // the properties in aAttributes are returned. 
                IntPtr aAttributes,

                // [out] Information about additional properties available to the caller; from the IFILTER_FLAGS enumeration. 
                out IFILTER_FLAGS pdwFlags);

            /// <summary>
            /// The IFilter::GetChunk method positions the filter at the beginning of the next chunk, or at the first chunk if this 
            /// is the first call to the GetChunk method, and returns a description of the current chunk. 
            /// </summary>
            [PreserveSig]
            IFilterReturnCode GetChunk(out STAT_CHUNK pStat);

            /// <summary>
            /// The IFilter::GetText method retrieves text (text-type properties) from the current chunk, 
            /// which must have a CHUNKSTATE enumeration value of CHUNK_TEXT.
            /// </summary>
            [PreserveSig]
            IFilterReturnCode GetText(
                // [in/out] On entry, the size of awcBuffer array in wide/Unicode characters. On exit, the number of Unicode 
                // characters written to awcBuffer. Note that this value is not the number of bytes in the buffer. 
                ref uint pcwcBuffer,

                // Text retrieved from the current chunk. Do not terminate the buffer with a character.  
                [Out, MarshalAs(UnmanagedType.LPArray)] char[] awcBuffer);

            /// <summary>
            /// The IFilter::GetValue method retrieves a value (public value-type property) from a chunk, 
            /// which must have a CHUNKSTATE enumeration value of CHUNK_VALUE.
            /// </summary>
            [PreserveSig]
            IFilterReturnCode GetValue(
                // Allocate the PROPVARIANT structure with CoTaskMemAlloc. Some PROPVARIANT structures contain pointers, 
                // which can be freed by calling the PropVariantClear function. It is up to the caller of the GetValue method 
                // to call the PropVariantClear method.            
                ref IntPtr pval);
                //[Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pval);

            /// <summary>
            /// The IFilter::BindRegion method retrieves an interface representing the specified portion of the object. 
            /// Currently reserved for future use.
            /// </summary>
            [PreserveSig]
            int BindRegion(ref FILTERREGION origPos, ref Guid riid, ref object ppunk);
        }
        #endregion

        #region Interface IClassFactory
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000001-0000-0000-C000-000000000046")]
        internal interface IClassFactory
        {
            void CreateInstance([MarshalAs(UnmanagedType.Interface)] object pUnkOuter, ref Guid refiid, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
            void LockServer(bool fLock);
        }
        #endregion

        #region Interface IPersist
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010c-0000-0000-C000-000000000046")]
        internal interface IPersist
        {
            void GetClassID( /* [out] */ out Guid pClassID);
        };

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000109-0000-0000-C000-000000000046")]
        internal interface IPersistStream : IPersist
        {
            new void GetClassID(out Guid pClassID);

            [PreserveSig]
            int IsDirty();
            void Load([In] IStream pStm);
            void Save([In] IStream pStm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax(out long pcbSize);
        };
        #endregion

        #region DllImports
        [DllImport("ole32.dll")]
        internal static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, out IStream ppstm);

        [
            System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"),
            DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)
        ]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint PSGetNameFromPropertyKey(
            ref PROPERTYKEY propkey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszCanonicalName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll")]
        internal static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        #endregion

        // ReSharper restore UnusedMember.Global
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnassignedField.Compiler
        // ReSharper restore UnusedField.Compiler
        // ReSharper restore InconsistentNaming
    }
}
