using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace FileStorage.Application.Dto
{
    public class GetFileListInput : PagedResultRequestDto
    {
        public Guid? ParentId { get; set; }

        public string ParentPath { get; set; }

        [Required]
        public string FileContainerName { get; set; }

        public long? OwnerUserId { get; set; }

        public bool DirectoryOnly { get; set; }

        public bool IncludeTrash { get; set; }

    }
}