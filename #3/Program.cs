using _1;
using _2;

namespace _3;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(extendedEuclideanAlgorithm, modularExponentiation, randomNumberGenerator);
        ISafePrimeParameterGenerator parameterGenerator = new SafePrimeParameterGenerator(randomNumberGenerator, primalityTest, modularExponentiation);
        IDiffieHellmanAlgorithm algorithm = new DiffieHellmanAlgorithm(modularExponentiation, primalityTest);
        IDiscreteLogarithmSolver discreteLogarithmSolver = new BabyStepGiantStepAlgorithm(modularExponentiation, extendedEuclideanAlgorithm, primalityTest);
        IDiffieHellmanScenarioService scenarioService = new DiffieHellmanScenarioService(parameterGenerator, randomNumberGenerator, algorithm);
        IKeyAgreementVerificationService verificationService = new KeyAgreementVerificationService(discreteLogarithmSolver);
        var ui = new ConsoleUi(scenarioService, verificationService);

        ui.Run();
    }
}
