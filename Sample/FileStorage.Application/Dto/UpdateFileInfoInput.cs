using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Extensions;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class UpdateFileInfoInput : UpdateFileBase, IValidatableObject
    {
        [Required]
        public string FileName { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (FileName.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("FileName should not be empty!",
                    new[] { nameof(FileName) });
            }

            FileName = FileName.Trim();
        }
    }
}