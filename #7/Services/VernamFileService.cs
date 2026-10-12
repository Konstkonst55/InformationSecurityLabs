using System.Numerics;
using Shared;

namespace _7;

public sealed class VernamFileService
{
    private readonly VernamAlgorithm _algorithm;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly VernamKeyFileService _keyFileService;

    public VernamFileService(
        VernamAlgorithm algorithm,
        IFileProcessingService fileProcessingService,
        VernamKeyFileService keyFileService)
    {
        _algorithm = algorithm;
        _fileProcessingService = fileProcessingService;
        _keyFileService = keyFileService;
    }

    public FileProcessResult EncryptWithOneTimeKey(
        string inputPath,
        string outputPath,
        string keyPath)
    {
        VernamKeyFileService.EnsureDifferentPaths(
            inputPath,
            outputPath,
            keyPath);
        _keyFileService.ValidateOneTimeKeyLength(inputPath, keyPath);

        using var keyStream = new FileStream(
            keyPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return _fileProcessingService.Encrypt(
            inputPath,
            outputPath,
            1,
            1,
            value => new BigInteger[]
            {
                _algorithm.Transform(value, ReadKeyByte(keyStream))
            });
    }

    public FileProcessResult DecryptWithOneTimeKey(
        string inputPath,
        string outputPath,
        string keyPath)
    {
        VernamKeyFileService.EnsureDifferentPaths(
            inputPath,
            outputPath,
            keyPath);
        _keyFileService.ValidateOneTimeKeyLength(inputPath, keyPath);

        using var keyStream = new FileStream(
            keyPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return _fileProcessingService.Decrypt(
            inputPath,
            outputPath,
            1,
            1,
            values =>
            {
                var value = ToByte(values[0]);
                return _algorithm.Transform(value, ReadKeyByte(keyStream));
            });
    }

    public FileProcessResult EncryptWithDiffieHellmanKey(
        string inputPath,
        string outputPath,
        string keyPath)
    {
        VernamKeyFileService.EnsureDifferentPaths(
            inputPath,
            outputPath,
            keyPath);
        var key = _keyFileService.LoadDiffieHellmanKey(keyPath);
        var keyStream = new VernamKeyStream(key);

        return _fileProcessingService.Encrypt(
            inputPath,
            outputPath,
            1,
            1,
            value => new BigInteger[]
            {
                _algorithm.Transform(value, keyStream.NextByte())
            });
    }

    public FileProcessResult DecryptWithDiffieHellmanKey(
        string inputPath,
        string outputPath,
        string keyPath)
    {
        VernamKeyFileService.EnsureDifferentPaths(
            inputPath,
            outputPath,
            keyPath);
        var key = _keyFileService.LoadDiffieHellmanKey(keyPath);
        var keyStream = new VernamKeyStream(key);

        return _fileProcessingService.Decrypt(
            inputPath,
            outputPath,
            1,
            1,
            values => _algorithm.Transform(
                ToByte(values[0]),
                keyStream.NextByte()));
    }

    private static byte ReadKeyByte(Stream keyStream)
    {
        var value = keyStream.ReadByte();
        if (value < 0)
        {
            throw new InvalidDataException(
                "Файл ключа неожиданно закончился.");
        }

        return (byte)value;
    }

    private static byte ToByte(BigInteger value)
    {
        if (value < byte.MinValue || value > byte.MaxValue)
        {
            throw new InvalidDataException(
                "Значение блока шифротекста выходит за диапазон байта.");
        }

        return (byte)value;
    }
}
