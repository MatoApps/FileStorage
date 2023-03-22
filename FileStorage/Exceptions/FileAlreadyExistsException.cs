using System;
using Abp;
using Abp.UI;

namespace FileStorage.Exceptions
{
    public class FileAlreadyExistsException : UserFriendlyException
    {
        public FileAlreadyExistsException(string filePath) : base(message: $"The file ({filePath}) already exists")
        {
        }

        public FileAlreadyExistsException(string fileName, Guid? parentId) : base(message: $"The file (name: {fileName}, parentId: {parentId}) already exists")
        {
        }
    }
}