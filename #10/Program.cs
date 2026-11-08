using SignatureShared;
using _1;
using _10.Algorithms;
using _10.Services;
using _10.UI;

namespace _10;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var parameterGenerator = new SignatureParameterGenerator(randomNumberGenerator, primalityTest, modularExponentiation);
        var gostAlgorithm = new Gost94SignatureAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, randomNumberGenerator, primalityTest);
        var dsaAlgorithm = new DsaSignatureAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, randomNumberGenerator, primalityTest);
        var fileService = new SignatureFileService(new FileHashService(), new SignatureDocumentStore(), gostAlgorithm, dsaAlgorithm);
        var consoleUi = new UI.ConsoleUi(parameterGenerator, fileService);

        consoleUi.Run();
    }
}
