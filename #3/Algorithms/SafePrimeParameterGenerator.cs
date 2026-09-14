using System.Numerics;
using _1;

namespace _3;

public sealed class SafePrimeParameterGenerator : ISafePrimeParameterGenerator
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IModularExponentiation _modularExponentiation;

    public SafePrimeParameterGenerator(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _modularExponentiation = modularExponentiation;
    }

    public DiffieHellmanParameters Generate(int bitLength, int primalityIterations = 20)
    {
        if (bitLength < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Разрядность должна быть не меньше 8.");
        }

        while (true)
        {
            var q = _randomNumberGenerator.GeneratePrime(bitLength - 1, _primalityTest, primalityIterations);
            var p = 2 * q + 1;

            if (!_primalityTest.IsProbablyPrime(p, primalityIterations))
            {
                continue;
            }

            var g = FindPrimitiveRoot(p, q);
            return new DiffieHellmanParameters(p, q, g);
        }
    }

    private BigInteger FindPrimitiveRoot(BigInteger p, BigInteger q)
    {
        for (var g = new BigInteger(2); g < p - 1; g++)
        {
            var gSquared = _modularExponentiation.Calculate(g, 2, p);
            var gToQ = _modularExponentiation.Calculate(g, q, p);

            if (gSquared != 1 && gToQ != 1)
            {
                return g;
            }
        }

        throw new InvalidOperationException("Не удалось найти первообразный корень.");
    }
}
