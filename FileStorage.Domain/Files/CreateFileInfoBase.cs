using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Abp.Application.Services.Dto;

namespace FileStorage.Files
{
    public class CreateFileInfoBase
    {
        public string FileContainerName { get; set; }

        public Guid? ParentId { get; set; }

        public string ParentPath { get; set; }

        public long? OwnerUserId { get; set; }



    }
}
