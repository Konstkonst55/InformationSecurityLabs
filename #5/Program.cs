using _1;
using Shared;

namespace _5;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);

        var parameterGenerator = new ElGamalParameterGenerator(randomNumberGenerator, primalityTest, modularExponentiation);
        var algorithm = new ElGamalAlgorithm(modularExponentiation, randomNumberGenerator, extendedEuclideanAlgorithm);
        IFileProcessingService fileProcessingService = new FileProcessingService();

        var ui = new ConsoleUi(algorithm, parameterGenerator, fileProcessingService);

        ui.Run();
    }
}
