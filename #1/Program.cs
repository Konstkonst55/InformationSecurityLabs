namespace _1;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);
        var ui = new ConsoleUi(modularExponentiation, primalityTest, extendedEuclideanAlgorithm, randomNumberGenerator);

        ui.Run();
    }
}
