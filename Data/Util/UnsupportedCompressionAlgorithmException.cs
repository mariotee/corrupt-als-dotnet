using System;
using System.Runtime.Serialization;

namespace server.Data.Util
{
    [Serializable]
    internal class UnsupportedCompressionAlgorithmException : Exception
    {
        public UnsupportedCompressionAlgorithmException()
        {
        }

        public UnsupportedCompressionAlgorithmException(string message) : base(message)
        {
        }

        public UnsupportedCompressionAlgorithmException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedCompressionAlgorithmException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}