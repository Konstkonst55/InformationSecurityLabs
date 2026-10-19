namespace SignatureShared;

public interface IFileHashService
{
    byte[] ComputeSha256(string filePath);
}
