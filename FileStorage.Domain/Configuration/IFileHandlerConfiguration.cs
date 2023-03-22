using System;
using System.Collections.Generic;
using Abp.Collections;
using Abp.Runtime.Validation.Interception;
using FileStorage.Domain.FileHandler;

namespace FileStorage.Configuration
{
    public interface IFileHandlerConfiguration
    {
        List<string> IgnoredTypes { get; }

        /// <summary>
        /// A list of method parameter validators.
        /// </summary>
        ITypeList<IFileHandler> Handlers { get; }
    }
}