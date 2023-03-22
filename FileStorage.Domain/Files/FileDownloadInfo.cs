using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace FileStorage.Application.Dto
{
    public class FileDownloadInfo
    {
        public string Token { get; set; }

        public string Mode { get; set; }
    }
}
