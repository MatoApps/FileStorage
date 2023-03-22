using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Abp.Extensions;
using Abp.Collections.Extensions;

namespace FileStorage.Application.Dto
{
    public class CreateManyFileInput : IValidatableObject
    {
        public List<CreateFileInput> FileInfos { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (FileInfos.IsNullOrEmpty())
            {
                yield return new ValidationResult("FileInfos should not be null or empty!",
                    new[] { nameof(FileInfos) });
            }

            if (FileInfos.Select(x => x.FileContainerName).Distinct().Count() > 1)
            {
                yield return new ValidationResult("FileContainerName of files should not be the same!",
                    new[] { nameof(CreateFileInput.FileContainerName) });
            }
        }
    }
}