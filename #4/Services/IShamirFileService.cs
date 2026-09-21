namespace _4;

public interface IShamirFileService
{
    EncryptionResult Encrypt(string inputPath, string outputPath, ShamirKeys keys);
    EncryptionResult Decrypt(string inputPath, string outputPath, ShamirKeys keys);
}
