using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFilterTextReader
{
    /// <summary>
    /// A class with options that control the way the <see cref="FilterReader"/> processes files
    /// </summary>
    public class FilterReaderOptions
    {
        /// <summary>
        /// When set to <c>true</c> then embedded content (e.g. an attachment in an E-mail) is not processed
        /// </summary>
        public bool DisableEmbeddedContent { get; set; }

        /// <summary>
        /// When set to <c>true</c> then file properties are also processed (e.g. properties in Word documents)
        /// </summary>
        public bool IncludeProperties { get; set; }

        /// <summary>
        /// When set to <c>true</c> then the file is read into memory before processing
        /// </summary>
        public bool ReadIntoMemory { get; set; }

        /// <summary>
        /// Controls the way the reader timeouts when processing files (default is no timeout)
        /// </summary>
        public FilterReaderTimeout FilterReaderTimeout { get; set; } 

        /// <summary>
        /// The amount of milliseconds before processing a file times out. This value is only
        /// used when <see cref="FilterReaderTimeout"/> is set to <see cref="FilterReaderTimeout.TimeoutOnly"/>
        /// or <see cref="FilterReaderTimeout.TimeoutWithException"/>
        /// </summary>
        public int Timeout { get; set; }
    }
}
