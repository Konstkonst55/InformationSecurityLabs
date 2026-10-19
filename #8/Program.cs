using SignatureShared;
using _1;

namespace _8;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var algorithm = new RsaSignatureAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, primalityTest);
        var fileService = new RsaSignatureFileService(new FileHashService(), new SignatureDocumentStore(), algorithm);
        var ui = new ConsoleUi(algorithm, fileService, randomNumberGenerator);

        ui.Run();
    }
}
