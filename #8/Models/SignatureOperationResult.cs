namespace _8;

public sealed record SignatureOperationResult(
    string InputPath,
    string SignaturePath,
    string HashHex,
    int SignedByteCount);
