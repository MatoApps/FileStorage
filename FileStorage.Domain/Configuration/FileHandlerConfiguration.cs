using System;
using System.Collections.Generic;
using Abp.Collections;
using Abp.Runtime.Validation.Interception;
using FileStorage.Domain.FileHandler;

namespace FileStorage.Configuration
{
    public class FileHandlerConfiguration : IFileHandlerConfiguration
    {
        public List<string> IgnoredTypes { get; }

        public ITypeList<IFileHandler> Handlers { get; }

        public FileHandlerConfiguration()
        {
            IgnoredTypes = new List<string>();
            Handlers = new TypeList<IFileHandler>();
        }
    }
}