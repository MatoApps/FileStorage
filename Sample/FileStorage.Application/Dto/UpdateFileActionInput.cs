using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Abp.Extensions;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class UpdateFileActionInput : UpdateFileBase, IValidatableObject
    {
        [Required]
        public string FileName { get; set; }

        public IFormFile File { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (FileName.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("FileName should not be empty!",
                    new[] { nameof(FileName) });
            }
        }
    }
}