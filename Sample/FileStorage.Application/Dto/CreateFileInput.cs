using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FileStorage.Enums;
using Microsoft.AspNetCore.Http;

namespace FileStorage.Application.Dto
{
    [Serializable]
    public class CreateFileInput : CreateFileBase
    {
        [Required]
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public FileType FileType { get; set; }

        public byte[] Content { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            base.Validate(validationContext);

            if (string.IsNullOrWhiteSpace(FileName))
            {
                yield return new ValidationResult("FileName should not be empty!",
                    new[] { nameof(FileName) });
            }

            if (!Enum.IsDefined(typeof(FileType), FileType))
            {
                yield return new ValidationResult("FileType is invalid!",
                    new[] { nameof(FileType) });
            }

            FileName = FileName.Trim();
        }
    }
}