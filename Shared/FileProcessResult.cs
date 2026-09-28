namespace Shared;

public sealed record FileProcessResult(
    string InputPath,
    string OutputPath,
    long ProcessedBytes);
