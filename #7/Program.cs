using Shared;
using _1;

namespace _7;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var diffieHellmanKeyGenerator = new DiffieHellmanKeyGenerator(modularExponentiation, randomNumberGenerator, primalityTest);
        var vernamAlgorithm = new VernamAlgorithm();
        var keyFileService = new VernamKeyFileService();
        var vernamFileService = new VernamFileService(vernamAlgorithm, new FileProcessingService(), keyFileService);

        new ConsoleUi(vernamFileService, keyFileService, diffieHellmanKeyGenerator).Run();
    }
}
