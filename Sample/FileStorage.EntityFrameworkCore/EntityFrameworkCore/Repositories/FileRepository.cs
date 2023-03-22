using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Abp;
using Abp.EntityFrameworkCore;
using Abp.Linq.Extensions;
using FileStorage.Enums;
using FileStorage.Files;
using FileStorage.Interfaces;

namespace FileStorage.EntityFrameworkCore.Repositories
{
    public class FileRepository : FileStorageRepositoryBase<File, Guid>, IFileRepository
    {
        public FileRepository(IDbContextProvider<FileStorageDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public virtual async Task<List<File>> GetListAsync(Guid? parentId, string fileContainerName, long? ownerUserId,
            FileType? specifiedFileType = null, CancellationToken cancellationToken = default)
        {
            return await (await GetTableAsync())
                .Where(x => x.ParentId == parentId && x.OwnerUserId == ownerUserId &&
                            x.FileContainerName == fileContainerName).WhereIf(specifiedFileType.HasValue,
                    x => x.FileType == specifiedFileType.Value).ToListAsync(cancellationToken);
        }

        public virtual async Task<File> FindAsync(string fileName, Guid? parentId, string fileContainerName, long? ownerUserId,
            CancellationToken cancellationToken = default)
        {
            return await (await GetTableAsync())
                .Where(x => x.ParentId == parentId && x.OwnerUserId == ownerUserId &&
                            x.FileContainerName == fileContainerName && x.FileName == fileName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<File> FirstOrDefaultAsync(string fileContainerName, string hash, long byteSize,
            CancellationToken cancellationToken = default)
        {
            return await (await GetTableAsync())
                .Where(x => x.Hash == hash && x.ByteSize == byteSize && x.FileContainerName == fileContainerName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<File> FirstOrDefaultAsync(string fileContainerName, string blobName,
            CancellationToken cancellationToken = default)
        {
            return await (await GetTableAsync()).Where(x => x.BlobName == blobName && x.FileContainerName == fileContainerName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<SubFilesStatisticDataModel> GetSubFilesStatisticDataAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            return await (await GetTableAsync()).Where(x => x.ParentId == id).GroupBy(x => true).Select(x =>
                new SubFilesStatisticDataModel
                {
                    SubFilesQuantity = x.Count(),
                    ByteSize = x.Sum(y => y.ByteSize)
                }).FirstOrDefaultAsync(cancellationToken) ?? new SubFilesStatisticDataModel();
        }

        public virtual async Task<string> GetFileNameWithNextSerialNumberAsync(string fileName, Guid? parentId, string fileContainerName, long? ownerUserId,
            CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

            var ext = System.IO.Path.GetExtension(fileName);

            var fileNameWithoutExt = fileName.Substring(0, fileName.LastIndexOf(ext, StringComparison.Ordinal));

            var part1 = fileNameWithoutExt + '(';

            var part2 = ')' + ext;

            var fileNames = await (await GetTableAsync())
                .Where(x => x.ParentId == parentId && x.OwnerUserId == ownerUserId &&
                            x.FileContainerName == fileContainerName && x.FileName.StartsWith(part1) &&
                            x.FileName.EndsWith(part2)).Select(x => x.FileName).ToListAsync(cancellationToken);

            var nextNumber =
                fileNames
                    .Select(x => x.Substring(part1.Length, x.LastIndexOf(part2, StringComparison.Ordinal) - part1.Length))
                    .Select(x => int.TryParse(x, out var number) ? number : 0).Where(x => x > 0).OrderBy(x => x)
                    .TakeWhile((x, i) => x == i + 1).LastOrDefault() + 1;

            return $"{part1}{nextNumber}{part2}";
        }
    }
}