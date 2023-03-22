using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Abp.Extensions;
using FileStorage.Enums;

namespace FileStorage.Application.Dto
{
    public class CreateManyFileActionInput : CreateFileBase, IValidatableObject
    {
        public FileType FileType { get; set; }

        public IFormFile[] Files { get; set; }

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