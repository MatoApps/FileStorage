using System;
using FileStorage.Files;
using File = FileStorage.Files.File;

namespace FileStorage.Exceptions
{
    public class IncorrectParentException : ApplicationException
    {
        public IncorrectParentException(File parent) : base($"The inputted parent (id: {parent?.Id}) entity is incorrect.")
        {
        }
    }
}