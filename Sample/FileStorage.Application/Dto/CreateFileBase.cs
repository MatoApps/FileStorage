using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Abp.Application.Services.Dto;

namespace FileStorage.Application.Dto
{
    public abstract class CreateFileBase :EntityDto<Guid>, IValidatableObject
    {
        [Required]
        public string FileContainerName { get; set; }

        public Guid? ParentId { get; set; }
        
        public string ParentPath { get; set; }

        public long? OwnerUserId { get; set; }


        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (string.IsNullOrWhiteSpace(FileContainerName))
            {
                yield return new ValidationResult("FileContainerName should not be empty!",
                    new[] { nameof(FileContainerName) });
            }
        }

    }
}
