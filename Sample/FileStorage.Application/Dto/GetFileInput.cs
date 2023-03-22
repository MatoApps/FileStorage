using Abp.Application.Services.Dto;
using System;

namespace FileStorage.Application.Dto
{
    public class GetFileInput : EntityDto<Guid>
    {
        public string Path { get; set; }

        public string FileContainerName { get; set; }

        public bool IncludeTrash { get; set; }

    }
}
