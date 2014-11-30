using System;
using System.Runtime.Serialization;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied
    /// </summary>
    [Serializable]
    public class IFOldFilterFormat : Exception
    {
        internal IFOldFilterFormat(SerializationInfo info, StreamingContext context) : base(info, context) { }

        internal IFOldFilterFormat() { }

        internal IFOldFilterFormat(string message) : base(message) { }

        internal IFOldFilterFormat(string message, Exception innerException) : base(message, innerException) { }
    }
}
