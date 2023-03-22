using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Extensions;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class UpdateFileInput : UpdateFileBase, IValidatableObject
    {
        [Required]
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public byte[] Content { get; set; }

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