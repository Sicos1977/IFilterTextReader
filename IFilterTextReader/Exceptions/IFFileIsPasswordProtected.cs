using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IFilterTextReader.Exceptions
{
    /// <summary>
    /// Raised when an IFilter can not read the file because it is password protected
    /// </summary>
    [Serializable]
    public class IFFileIsPasswordProtected : Exception
    {
        protected IFFileIsPasswordProtected(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IFFileIsPasswordProtected() { }

        public IFFileIsPasswordProtected(string message) : base(message) { }

        public IFFileIsPasswordProtected(string message, Exception innerException) : base(message, innerException) { }
    }
}
