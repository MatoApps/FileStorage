using System;

namespace FileStorage.Application.Dto
{
    public class CreateLocalInput: CreateFileBase
    {
        public string ImportDir { get; set; }
        public bool GenerateUniqueFileName { get;  set; }
        public string ExclusionDirs { get;  set; }
    }
}