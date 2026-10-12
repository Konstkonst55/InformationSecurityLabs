using Shared;
using _1;

namespace _6;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var algorithm = new RsaAlgorithm(modularExponentiation, extendedEuclideanAlgorithm);
        IFileProcessingService fileProcessingService = new FileProcessingService();

        var ui = new ConsoleUi(algorithm, randomNumberGenerator, primalityTest, fileProcessingService);

        ui.Run();
    }
}
