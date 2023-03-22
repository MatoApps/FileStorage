using System;
using Abp;
using Abp.UI;

namespace FileStorage.Exceptions
{
    public class DirectoryFileContentIsNotEmptyException : UserFriendlyException
    {
        public DirectoryFileContentIsNotEmptyException() : base(message: "Content should be empty if the file is a directory.")
        {
        }
    }
}