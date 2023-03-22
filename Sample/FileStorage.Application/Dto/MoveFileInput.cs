using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Extensions;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class MoveFileInput : UpdateFileBase, IValidatableObject
    {
        public Guid? NewParentId { get; set; }
        public string NewParentPath { get; set; }
        public string FileContainerName { get; set; }

        [Required]
        public string NewFileName { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NewFileName.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("NewFileName should not be empty!",
                    new[] { nameof(NewFileName) });
            }

            NewFileName = NewFileName.Trim();
        }
    }
}