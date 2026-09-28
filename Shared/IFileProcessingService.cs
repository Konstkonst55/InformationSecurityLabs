using System.Numerics;

namespace Shared;

public interface IFileProcessingService
{
    FileProcessResult Encrypt(
        string inputPath,
        string outputPath,
        int blockWidth,
        int blocksPerByte,
        Func<byte, IReadOnlyList<BigInteger>> transform);

    FileProcessResult Decrypt(
        string inputPath,
        string outputPath,
        int blockWidth,
        int blocksPerByte,
        Func<IReadOnlyList<BigInteger>, byte> transform);
}
