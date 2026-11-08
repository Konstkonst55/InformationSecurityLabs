using System.Text.Json;

namespace SignatureShared;

public sealed class SignatureDocumentStore : ISignatureDocumentStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public void Save(string filePath, SignatureDocument document)
    {
        ValidatePath(filePath);
        ArgumentNullException.ThrowIfNull(document);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, JsonSerializer.Serialize(document, SerializerOptions));
    }

    public SignatureDocument Load(string filePath)
    {
        ValidatePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл подписи не найден.", filePath);
        }

        var document = JsonSerializer.Deserialize<SignatureDocument>(File.ReadAllText(filePath), SerializerOptions);
        if (document is null ||
            string.IsNullOrWhiteSpace(document.Algorithm) ||
            string.IsNullOrWhiteSpace(document.HashAlgorithm) ||
            document.PublicParameters is null ||
            document.PublicParameters.Count == 0 ||
            document.SignatureValues is null ||
            document.SignatureValues.Count != 1 ||
            document.SignatureValues[0] is null ||
            document.SignatureValues[0].Count != 2)
        {
            throw new InvalidDataException("Файл подписи имеет неверный формат.");
        }

        return document;
    }

    private static void ValidatePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу не задан.", nameof(filePath));
        }
    }
}
