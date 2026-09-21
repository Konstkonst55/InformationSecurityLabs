using System.Numerics;

namespace _4;

public sealed record EncryptionResult(
    string InputPath,
    string OutputPath,
    BigInteger P,
    BigInteger Ca,
    BigInteger Da,
    BigInteger Cb,
    BigInteger Db,
    long ProcessedBytes);
