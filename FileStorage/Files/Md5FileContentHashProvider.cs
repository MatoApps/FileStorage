﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Abp.Dependency;
using FileStorage.Interfaces;

namespace FileStorage.Files
{
    public class Md5FileContentHashProvider : IFileContentHashProvider, ITransientDependency
    {
        public virtual string GetHashString(byte[] fileContent)
        {
            if (fileContent == null)
            {
                return null;
            }

            var md5 = GetMd5(fileContent);

            return md5.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }

        protected virtual IEnumerable<byte> GetMd5(byte[] fileContent)
        {
            MD5 hashString = new MD5CryptoServiceProvider();
            return hashString.ComputeHash(fileContent);
        }
    }
}