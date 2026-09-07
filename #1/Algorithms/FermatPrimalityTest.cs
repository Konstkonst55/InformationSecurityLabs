using System.Numerics;

namespace _1;

public sealed class FermatPrimalityTest : IPrimalityTest
{
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public FermatPrimalityTest(
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IModularExponentiation modularExponentiation,
        IRandomNumberGenerator randomNumberGenerator)
    {
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
        _modularExponentiation = modularExponentiation;
        _randomNumberGenerator = randomNumberGenerator;
    }

    public bool IsProbablyPrime(BigInteger number, int iterations = 100)
    {
        if (iterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations), "Число итераций должно быть положительным.");
        }

        if (number < 2)
        {
            return false;
        }

        if (number == 2)
        {
            return true;
        }

        if (number.IsEven)
        {
            return false;
        }

        for (var i = 0; i < iterations; i++)
        {
            var a = _randomNumberGenerator.Generate(2, number - 1);
            var gcd = _extendedEuclideanAlgorithm.Calculate(a, number).Gcd;

            if (gcd != 1)
            {
                return false;
            }

            if (_modularExponentiation.Calculate(a, number - 1, number) != 1)
            {
                return false;
            }
        }

        return true;
    }
}
