namespace SignatureShared;

public interface ISignatureDocumentStore
{
    void Save(string filePath, SignatureDocument document);
    SignatureDocument Load(string filePath);
}
