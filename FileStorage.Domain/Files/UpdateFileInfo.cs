using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Extensions;

namespace FileStorage.Files
{
    public class UpdateFileInfo : Entity<Guid>
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public byte[] Content { get; set; }

    }
}