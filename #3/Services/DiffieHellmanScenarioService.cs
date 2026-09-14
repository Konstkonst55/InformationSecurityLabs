using System.Numerics;
using _1;

namespace _3;

public sealed class DiffieHellmanScenarioService : IDiffieHellmanScenarioService
{
    private readonly ISafePrimeParameterGenerator _parameterGenerator;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IDiffieHellmanAlgorithm _algorithm;

    public DiffieHellmanScenarioService(
        ISafePrimeParameterGenerator parameterGenerator,
        IRandomNumberGenerator randomNumberGenerator,
        IDiffieHellmanAlgorithm algorithm)
    {
        _parameterGenerator = parameterGenerator;
        _randomNumberGenerator = randomNumberGenerator;
        _algorithm = algorithm;
    }

    public DiffieHellmanResult GenerateScenario(int bitLength)
    {
        var parameters = _parameterGenerator.Generate(bitLength);
        var alicePrivateKey = _randomNumberGenerator.Generate(2, parameters.P - 2);
        var bobPrivateKey = _randomNumberGenerator.Generate(2, parameters.P - 2);

        return _algorithm.Calculate(
            parameters.P,
            parameters.G,
            alicePrivateKey,
            bobPrivateKey);
    }

    public DiffieHellmanResult CalculateScenario(
        BigInteger p,
        BigInteger g,
        BigInteger alicePrivateKey,
        BigInteger bobPrivateKey)
    {
        return _algorithm.Calculate(p, g, alicePrivateKey, bobPrivateKey);
    }
}
