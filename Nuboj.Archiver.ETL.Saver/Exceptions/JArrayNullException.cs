using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Nuboj.Archiver.ETL.Saver.Exceptions
{
    public class JArrayNullException : Exception
    {
        public JArrayNullException()
        {
        }

        public JArrayNullException(string? message) : base(message)
        {
        }

        public JArrayNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected JArrayNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
