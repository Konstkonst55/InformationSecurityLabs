namespace SignatureShared;

public sealed record SignatureDocument
{
    public string Algorithm { get; init; } = string.Empty;
    public string HashAlgorithm { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string HashHex { get; init; } = string.Empty;
    public Dictionary<string, string> PublicParameters { get; init; } = new();
    public List<List<string>> SignatureValues { get; init; } = new();
}
