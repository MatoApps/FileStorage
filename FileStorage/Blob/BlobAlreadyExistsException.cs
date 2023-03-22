using System;
using System.Runtime.Serialization;
using Abp;

namespace FileStorage.Blob
{
    public class BlobAlreadyExistsException : AbpException
    {
        public BlobAlreadyExistsException()
        {

        }

        public BlobAlreadyExistsException(string message)
            : base(message)
        {

        }

        public BlobAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public BlobAlreadyExistsException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }
    }
}