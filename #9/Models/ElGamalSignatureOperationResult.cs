namespace _9;

public sealed record ElGamalSignatureOperationResult(
    string InputPath,
    string SignaturePath,
    string HashHex,
    int SignedByteCount);
