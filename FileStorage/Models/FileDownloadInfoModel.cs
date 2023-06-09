﻿using System;

namespace FileStorage.Models
{
    [Serializable]
    public class FileDownloadInfoModel
    {
        public string DownloadMethod { get; set; }

        public string DownloadUrl { get; set; }

        public string ExpectedFileName { get; set; }

        public string Token { get; set; }
        public string ResUrl { get; set; }
    }
}