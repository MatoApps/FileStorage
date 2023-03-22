using System;
using FileStorage.Files;
using File = FileStorage.Files.File;

namespace FileStorage.Files
{
    public class FileOperationInfoModel
    {
        public Guid? ParentId { get; set; }

        public string FileContainerName { get; set; }

        public long? OwnerUserId { get; set; }

        public File File { get; set; }
    }
}