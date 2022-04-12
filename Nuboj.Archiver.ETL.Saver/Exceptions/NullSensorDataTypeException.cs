using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Nuboj.Archiver.ETL.Saver.Exceptions
{
    public class NullSensorDataTypeException : Exception
    {
        public NullSensorDataTypeException()
        {
        }

        public NullSensorDataTypeException(string? message) : base(message)
        {
        }

        public NullSensorDataTypeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NullSensorDataTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
