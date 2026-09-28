using _1;
using Shared;

namespace _4;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var algorithm = new ShamirAlgorithm(modularExponentiation);
        IFileProcessingService fileProcessingService = new FileProcessingService();
        var ui = new ConsoleUi(algorithm, randomNumberGenerator, primalityTest, extendedEuclideanAlgorithm, fileProcessingService);

        ui.Run();
    }
}
