using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FileStorage.Enums;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Application.Dto
{
    public class CreateFileActionInput : CreateFileBase, IValidatableObject
    {
        public FileType FileType { get; set; }

        public IFormFile File { get; set; }

        public bool GenerateUniqueFileName { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            base.Validate(validationContext);

            if (!Enum.IsDefined(typeof(FileType), FileType))
            {
                yield return new ValidationResult("FileType is invalid!",
                    new[] { nameof(FileType) });
            }
        }
    }
}