using System;
using Abp.Domain.Entities;
using Abp.Events.Bus;
using Abp.MultiTenancy;
using FileStorage.Enums;

namespace FileStorage.Files
{
    [Serializable]
    public class FileBlobNameChangedEto : EventData, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public Guid FileId { get; set; }

        public FileType FileType { get; set; }

        public string FileContainerName { get; set; }

        public string OldBlobName { get; set; }

        public string NewBlobName { get; set; }
    }
}