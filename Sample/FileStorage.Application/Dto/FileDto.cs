using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using FileStorage.Enums;
using FileStorage.Files;

namespace FileStorage.Application.Dto
{
    [AutoMapTo(typeof(File))]
    [AutoMapFrom(typeof(File))]

    internal class FileDto : FullAuditedEntityDto<Guid>
    {

        public  Guid? ParentId { get; set; }

        public  string FileContainerName { get; protected set; }

        public  string FileName { get; protected set; }

        public  string MimeType { get; protected set; }

        public  FileType FileType { get; protected set; }

        public  int SubFilesQuantity { get; protected set; }

        public  long ByteSize { get; protected set; }

        public  string Hash { get; protected set; }

        public  string BlobName { get; protected set; }

        public  long? OwnerUserId { get; protected set; }

        public  string Flag { get; protected set; }

    }
}
