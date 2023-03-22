using System;
using System.IO;

namespace FileStorage.Application.Dto
{

    public interface IRemoteStreamContent : IDisposable
    {
        string FileName { get; }

        string ContentType { get; }

        long? ContentLength { get; }

        Stream GetStream();
    }
}