using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using FileStorage.Files;
using FileStorage.Enums;

namespace FileStorage.Application.Dto
{
    [AutoMapFrom(typeof(File))]
    public class FileBriefWithThumbnailDto : FileBriefDto
    {
        public object? Thumbnail { get; set; }

    }
}