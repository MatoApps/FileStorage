using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FileStorage.Enums;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Files
{
    public class CreateFileInfo : CreateFileInfoBase
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public FileType FileType { get; set; }

        public byte[] Content { get; set; }

    }
}