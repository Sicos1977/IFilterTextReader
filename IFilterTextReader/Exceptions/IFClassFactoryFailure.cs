using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when it is not possible to get the class object from an IFilter
    /// </summary>
    [Serializable]
    public class IFClassFactoryFailure : Exception
    {
        protected IFClassFactoryFailure(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IFClassFactoryFailure() { }

        public IFClassFactoryFailure(string message) : base(message) { }

        public IFClassFactoryFailure(string message, Exception innerException) : base(message, innerException) { }
    }
}
