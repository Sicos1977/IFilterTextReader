using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when a file or IFilter cannot be accessed 
    /// </summary>
    [Serializable]
    public class IFAccesFailure : Exception
    {
        protected IFAccesFailure(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IFAccesFailure() { }

        public IFAccesFailure(string message) : base(message) { }

        public IFAccesFailure(string message, Exception innerException) : base(message, innerException) { }
    }
}
