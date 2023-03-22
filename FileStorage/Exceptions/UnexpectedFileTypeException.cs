using System;
using Abp.UI;
using FileStorage.Enums;

namespace FileStorage.Exceptions
{
    public class UnexpectedFileTypeException : UserFriendlyException
    {
        public UnexpectedFileTypeException(Guid fileId, FileType fileType) : base(
            message: $"The type ({fileType}) of the file ({fileId}) is unexpected.")
        {
        }

        public UnexpectedFileTypeException(Guid fileId, FileType fileType, FileType expectedFileType) : base(
            message: $"The type ({fileType}) of the file ({fileId}) is unexpected, it should be {expectedFileType}.")
        {
        }
    }
}