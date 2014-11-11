using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when there is not IFilter installed for a file
    /// </summary>
    [Serializable]
    public class IFFilterNotFound : Exception
    {
        protected IFFilterNotFound(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IFFilterNotFound() { }

        public IFFilterNotFound(string message) : base(message) { }

        public IFFilterNotFound(string message, Exception innerException) : base(message, innerException) { }
    }
}
