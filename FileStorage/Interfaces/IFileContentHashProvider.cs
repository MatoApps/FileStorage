namespace FileStorage.Interfaces
{
    public interface IFileContentHashProvider
    {
        string GetHashString(byte[] fileContent);
    }
}