using _1;

namespace _2;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);
        IDiscreteLogarithmSolver solver = new BabyStepGiantStepAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, primalityTest);
        var scenarioService = new DiscreteLogScenarioService(randomNumberGenerator, primalityTest, modularExponentiation, solver);
        IVerificationService verificationService = new DiscreteLogVerificationService(modularExponentiation);
        var ui = new ConsoleUi(solver, scenarioService, verificationService);

        ui.Run();
    }
}
