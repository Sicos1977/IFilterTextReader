namespace IFilterTextReader
{
    /// <summary>
    /// A class with options that control the way the <see cref="FilterReader"/> processes files
    /// </summary>
    public class FilterReaderOptions
    {
        #region Properties
        /// <summary>
        /// When set to <c>true</c> the <see cref="NativeMethods.IFilter"/>
        /// doesn't read embedded content, e.g. an attachment inside an E-mail msg file. This parameter is default set to <c>false</c>
        /// </summary>
        public bool DisableEmbeddedContent { get; set; }

        /// <summary>
        /// When set to <c>true</c> the metadata properties of
        /// a document are also returned, e.g. the summary properties of a Word document. This parameter
        /// is default set to <c>false</c>
        /// </summary>
        public bool IncludeProperties { get; set; }

        /// <summary>
        /// When set to <c>true</c> the file to process is completely read 
        /// into memory first before the iFilters starts to read chunks, when set to <c>false</c> the iFilter reads
        /// directly from the file and advances reading when the chunks are returned. 
        /// Default set to <c>false</c>
        /// </summary>
        public bool ReadIntoMemory { get; set; }

        /// <summary>
        /// Can be used to timeout when parsing very large files, default set to <see cref="FilterReaderTimeout.NoTimeout"/>
        /// </summary>
        public FilterReaderTimeout ReaderTimeout { get; set; }

        /// <summary>
        /// The timeout in millisecond when the <see cref="FilterReaderTimeout"/> is set to a value other then <see cref="FilterReaderTimeout.NoTimeout"/>
        /// </summary>
        /// <remarks>This value is only
        /// used when <see cref="FilterReaderTimeout"/> is set to <see cref="FilterReaderTimeout.TimeoutOnly"/>
        /// or <see cref="FilterReaderTimeout.TimeoutWithException"/>
        /// </remarks>
        public int Timeout { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates this object and sets it's defaults properties
        /// </summary>
        public FilterReaderOptions()
        {
            DisableEmbeddedContent = false;
            IncludeProperties = false;
            ReadIntoMemory = false;
            ReaderTimeout = FilterReaderTimeout.NoTimeout;
            Timeout = -1;
        }
        #endregion
    }
}
