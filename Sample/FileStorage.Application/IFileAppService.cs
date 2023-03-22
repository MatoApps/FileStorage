using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using FileStorage;
using FileStorage.Models;
using FileStorage.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Application
{
    public interface IFileAppService

    {
        Task<FileBriefDto> CreateAsync(CreateFileInput input);

        Task<CreateFileOutput> CreateWithStreamAsync(CreateFileWithStreamInput input);


        Task<CreateManyFileOutput> CreateManyWithStreamAsync(CreateManyFileWithStreamInput input);

        Task<FileBriefDto> UpdateAsync( UpdateFileInput input);

        Task<FileBriefDto> UpdateWithStreamAsync( UpdateFileWithStreamInput input);

        Task<FileBriefDto> MoveAsync( MoveFileInput input);

        Task DeleteAsync(GetFileInput input);
        Task<FileBriefDto> GetAsync(GetFileInput input);
        Task<FileDownloadInfoModel> GetDownloadInfoAsync(EntityDto<Guid> input);

        Task<FileBriefDto> UpdateInfoAsync( UpdateFileInfoInput input);
        Task<PagedResultDto<FileBriefDto>> GetAllAsync(GetFileListInput input);

        Task<PublicFileContainerConfiguration> GetConfigurationAsync(string fileContainerName, long? ownerUserId);
        Task<CreateFileOutput> ActionCreateAsync([FromForm] CreateFileActionInput input);
        Task<FileBriefDto> ActionUpdateAsync([FromForm] UpdateFileActionInput input);
        Task CreateLocalAsync(CreateLocalInput input);
        Task<IActionResult> ActionGetThumbnailImageAsync(Guid id);
        Task<IActionResult> ActionDownloadAsync(FileDownloadInput input);
        Task<CreateManyFileOutput> ActionCreateManyAsync([FromForm] CreateManyFileActionInput input);
        Task<FileBriefDto> CopyAsync(MoveFileInput input);
        Task<PagedResultDto<FileBriefWithThumbnailDto>> GetAllWithThumbnailAsync(GetFileListInput input);
    }
}