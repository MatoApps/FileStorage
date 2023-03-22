using System;
using FileStorage.Models;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class CreateFileOutput
    {
        public FileBriefDto FileInfo { get; set; }

        public FileDownloadInfoModel DownloadInfo { get; set; }
    }
}