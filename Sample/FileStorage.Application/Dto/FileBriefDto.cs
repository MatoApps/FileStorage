using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using FileStorage.Enums;
using FileStorage.Files;

namespace FileStorage.Application.Dto
{
    [AutoMapFrom(typeof(File))]
    public class FileBriefDto : FullAuditedEntityDto<Guid>
    {
        public Guid? ParentId { get; set; }

        public string FileContainerName { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public FileType FileType { get; set; }

        public int SubFilesQuantity { get; set; }

        public long ByteSize { get; set; }

        public string Hash { get; set; }

        public long? OwnerUserId { get; set; }

    }
}