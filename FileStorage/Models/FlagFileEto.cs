using System;
using Abp.Events.Bus;
using JetBrains.Annotations;

namespace FileStorage.Models
{
    [Serializable]
    public class FlagFileEto
    {
        public Guid FileId { get; set; }

        [CanBeNull]
        public string Flag { get; set; }
    }
}