using System;
using Abp.UI;

namespace FileStorage.Exceptions
{
    public class FileNameContainsSeparatorException : UserFriendlyException
    {
        public FileNameContainsSeparatorException(string fileName, char separator) : base(
            message: $"The file name ({fileName}) should not contains the separator ({separator}).")
        {
        }
    }
}