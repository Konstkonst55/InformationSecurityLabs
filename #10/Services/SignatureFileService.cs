using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using SignatureShared;
using _10.Algorithms;
using _10.Models;

namespace _10.Services;

public sealed class SignatureFileService
{
    private readonly IFileHashService _hashService;
    private readonly ISignatureDocumentStore _signatureDocumentStore;
    private readonly Gost94SignatureAlgorithm _gostAlgorithm;
    private readonly DsaSignatureAlgorithm _dsaAlgorithm;

    public SignatureFileService(
        IFileHashService hashService,
        ISignatureDocumentStore signatureDocumentStore,
        Gost94SignatureAlgorithm gostAlgorithm,
        DsaSignatureAlgorithm dsaAlgorithm)
    {
        _hashService = hashService;
        _signatureDocumentStore = signatureDocumentStore;
        _gostAlgorithm = gostAlgorithm;
        _dsaAlgorithm = dsaAlgorithm;
    }

    public SignatureOperationResult SignFile(
        string inputFilePath,
        string signatureFilePath,
        SignatureKeyPair keyPair)
    {
        ValidateInputFile(inputFilePath);
        ArgumentNullException.ThrowIfNull(keyPair);
        ValidateSignaturePath(inputFilePath, signatureFilePath);

        var hash = _hashService.ComputeSha256(inputFilePath);
        var signature = keyPair.Algorithm switch
        {
            Gost94SignatureAlgorithm.AlgorithmName => _gostAlgorithm.SignHash(hash, keyPair),
            DsaSignatureAlgorithm.AlgorithmName => _dsaAlgorithm.SignHash(hash, keyPair),
            _ => throw new InvalidDataException("Неизвестный алгоритм подписи.")
        };

        var document = new SignatureDocument
        {
            Algorithm = keyPair.Algorithm,
            HashAlgorithm = "SHA-256",
            OriginalFileName = Path.GetFileName(inputFilePath),
            HashHex = Convert.ToHexString(hash),
            PublicParameters = keyPair.GetPublicParameters(),
            SignatureValues =
            [
                [
                    signature.R.ToString(CultureInfo.InvariantCulture),
                    signature.S.ToString(CultureInfo.InvariantCulture)
                ]
            ]
        };

        _signatureDocumentStore.Save(signatureFilePath, document);
        return new SignatureOperationResult(signatureFilePath, Convert.ToHexString(hash), hash.Length);
    }

    public SignatureVerificationResult VerifyFile(string inputFilePath, string signatureFilePath)
    {
        ValidateInputFile(inputFilePath);
        if (string.IsNullOrWhiteSpace(signatureFilePath))
        {
            throw new ArgumentException("Путь к файлу подписи не задан.", nameof(signatureFilePath));
        }

        var document = _signatureDocumentStore.Load(signatureFilePath);
        if (!string.Equals(document.HashAlgorithm, "SHA-256", StringComparison.OrdinalIgnoreCase))
        {
            return new SignatureVerificationResult(false, "В файле подписи указан неподдерживаемый алгоритм хеширования.");
        }

        var actualHash = _hashService.ComputeSha256(inputFilePath);
        byte[] savedHash;
        try
        {
            savedHash = Convert.FromHexString(document.HashHex);
        }
        catch (FormatException)
        {
            return new SignatureVerificationResult(false, "В файле подписи записан некорректный хэш.");
        }

        if (savedHash.Length != actualHash.Length ||
            !CryptographicOperations.FixedTimeEquals(savedHash, actualHash))
        {
            return new SignatureVerificationResult(false, "Файл изменен после подписания: хэши не совпадают.");
        }

        if (document.SignatureValues.Count != 1)
        {
            return new SignatureVerificationResult(false, "Файл подписи должен содержать одну пару значений r и s.");
        }

        var values = document.SignatureValues[0];
        if (values.Count != 2 ||
            !BigInteger.TryParse(values[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ||
            !BigInteger.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var s))
        {
            return new SignatureVerificationResult(false, "Значения подписи имеют неверный формат.");
        }

        var isValid = document.Algorithm switch
        {
            Gost94SignatureAlgorithm.AlgorithmName => _gostAlgorithm.VerifyHash(
                actualHash, r, s, document.PublicParameters),
            DsaSignatureAlgorithm.AlgorithmName => _dsaAlgorithm.VerifyHash(
                actualHash, r, s, document.PublicParameters),
            _ => false
        };

        return isValid
            ? new SignatureVerificationResult(true, "Подпись корректна. Файл не изменен после подписания.")
            : new SignatureVerificationResult(false, "Подпись недействительна.");
    }

    private static void ValidateInputFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу не задан.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Исходный файл не найден.", filePath);
        }
    }

    private static void ValidateSignaturePath(string inputPath, string signaturePath)
    {
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
