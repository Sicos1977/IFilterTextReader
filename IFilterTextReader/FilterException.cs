using System;
using System.Runtime.Serialization;

namespace Email2Storage.Modules.Readers.TextReader
{
    /// <summary>
    /// Deze exceptie wordt geraised wanneer er een Filter gerelateerde foutmelding optreed
    /// </summary>
    [Serializable]
    public class FilterException : Exception
    {
        protected FilterException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public FilterException() { }

        public FilterException(string message) : base(message) { }

        public FilterException(string message, Exception innerException) : base(message, innerException) { }
    }
}