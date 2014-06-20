using System;
using System.Runtime.Serialization;

namespace Email2Storage.Modules.Readers.IFilterTextReader
{
    /// <summary>
    /// This exception is raised when something goes wrong with an IFilter
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