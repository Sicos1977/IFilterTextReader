using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when the file to process is not in the format the IFilter would expect it to be
    /// </summary>
    [Serializable]
    public class IFUnknownFormat : Exception
    {
        protected IFUnknownFormat(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IFUnknownFormat() { }

        public IFUnknownFormat(string message) : base(message) { }

        public IFUnknownFormat(string message, Exception innerException) : base(message, innerException) { }
    }
}
