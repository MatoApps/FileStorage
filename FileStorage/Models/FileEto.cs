using System;
using Abp.Domain.Entities;
using FileStorage.Enums;

namespace FileStorage.Models
{

    public class FileEto : IMayHaveTenant
    {
        public Guid Id { get; set; }

        public int? TenantId { get; set; }

        public Guid? ParentId { get; set; }

        public string FileContainerName { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public FileType FileType { get; set; }

        public int SubFilesQuantity { get; set; }

        public long ByteSize { get; set; }

        public string Hash { get; set; }

        public string BlobName { get; set; }

        public long? OwnerUserId { get; set; }

        public string Flag { get; set; }
    }
}