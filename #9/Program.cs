using SignatureShared;
using _1;

namespace _9;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var keyGenerator = new ElGamalSignatureKeyGenerator(randomNumberGenerator, primalityTest, modularExponentiation);
        var algorithm = new ElGamalSignatureAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, randomNumberGenerator, primalityTest);
        var fileService = new ElGamalSignatureFileService(new FileHashService(), new SignatureDocumentStore(), algorithm);

        var ui = new ConsoleUi(algorithm, keyGenerator, fileService);
        ui.Run();
    }
}
