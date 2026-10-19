using SignatureShared;

namespace _8;

public sealed class RsaSignatureFileService
{
    private readonly IFileHashService _fileHashService;
    private readonly ISignatureDocumentStore _signatureDocumentStore;
    private readonly RsaSignatureAlgorithm _algorithm;

    public RsaSignatureFileService(
        IFileHashService fileHashService,
        ISignatureDocumentStore signatureDocumentStore,
        RsaSignatureAlgorithm algorithm)
    {
        _fileHashService = fileHashService;
        _signatureDocumentStore = signatureDocumentStore;
        _algorithm = algorithm;
    }

    public SignatureOperationResult SignFile(
        string inputPath,
        string signaturePath,
        RsaSignatureKeys keys)
    {
        ValidatePaths(inputPath, signaturePath);
        var hash = _fileHashService.ComputeSha256(inputPath);
        var document = _algorithm.SignHash(hash, inputPath, keys);
        _signatureDocumentStore.Save(signaturePath, document);

        return new SignatureOperationResult(
            inputPath,
            signaturePath,
            document.HashHex,
            document.SignatureValues.Count);
    }

    public bool VerifyFile(string inputPath, string signaturePath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Проверяемый файл не найден.", inputPath);
        }

        var document = _signatureDocumentStore.Load(signaturePath);
        var hash = _fileHashService.ComputeSha256(inputPath);
        if (!string.Equals(document.HashHex, Convert.ToHexString(hash), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _algorithm.VerifyHash(hash, document);
    }

    private static void ValidatePaths(string inputPath, string signaturePath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Путь к исходному файлу не задан.", nameof(inputPath));
        }

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Подписываемый файл не найден.", inputPath);
        }

        if (string.IsNullOrWhiteSpace(signaturePath))
        {
            throw new ArgumentException("Путь к файлу подписи не задан.", nameof(signaturePath));
        }

        if (string.Equals(
                Path.GetFullPath(inputPath),
                Path.GetFullPath(signaturePath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Файл подписи должен отличаться от подписываемого файла.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(signaturePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
