using System;
using Abp.Application.Services.Dto;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class CreateManyFileOutput : ListResultDto<CreateFileOutput>
    {
    }
}