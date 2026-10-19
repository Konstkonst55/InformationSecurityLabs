using System.Security.Cryptography;

namespace SignatureShared;

public sealed class FileHashService : IFileHashService
{
    public byte[] ComputeSha256(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу не задан.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл не найден.", filePath);
        }

        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return SHA256.HashData(stream);
    }
}
