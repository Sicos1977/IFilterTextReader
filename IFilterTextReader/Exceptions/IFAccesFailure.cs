using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when the IFilter is unable to acces an object
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