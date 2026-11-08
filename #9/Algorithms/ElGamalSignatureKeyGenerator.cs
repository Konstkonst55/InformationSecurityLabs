using System.Numerics;
using _1;

namespace _9;

public sealed class ElGamalSignatureKeyGenerator
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IModularExponentiation _modularExponentiation;

    public ElGamalSignatureKeyGenerator(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _modularExponentiation = modularExponentiation;
    }

    public ElGamalSignatureKeyPair Generate(int bitLength = 32, int primalityIterations = 40)
    {
        if (bitLength < 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bitLength),
                "Разрядность P должна быть не меньше 10.");
        }

        while (true)
        {
            var q = _randomNumberGenerator.GeneratePrime(
                bitLength - 1,
                _primalityTest,
                primalityIterations);
            var p = 2 * q + 1;

            if (!_primalityTest.IsProbablyPrime(p, primalityIterations) || p <= 257)
            {
                continue;
            }

            var g = FindPrimitiveRoot(p, q);
            var x = _randomNumberGenerator.Generate(2, p - 2);
            var y = _modularExponentiation.Calculate(g, x, p);
            var publicKey = new ElGamalSignaturePublicKey(p, q, g, y);

            return new ElGamalSignatureKeyPair(publicKey, x);
        }
    }

    public ElGamalSignatureKeyPair Create(
        BigInteger p,
        BigInteger g,
        BigInteger x)
    {
        ValidateSafePrime(p);
        var q = (p - 1) / 2;
        ValidatePrimitiveRoot(p, q, g);

        if (x <= 1 || x >= p - 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "Секретный ключ x должен удовлетворять условию 1 < x < P - 1.");
        }

        var y = _modularExponentiation.Calculate(g, x, p);
        return new ElGamalSignatureKeyPair(
            new ElGamalSignaturePublicKey(p, q, g, y),
            x);
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

    private void ValidateSafePrime(BigInteger p)
    {
        if (p <= 257 || !_primalityTest.IsProbablyPrime(p, 40))
        {
            throw new ArgumentException(
                "P должно быть простым числом больше 257.",
                nameof(p));
        }

        var q = (p - 1) / 2;
        if (2 * q + 1 != p || !_primalityTest.IsProbablyPrime(q, 40))
        {
            throw new ArgumentException(
                "P должно удовлетворять условию P = 2Q + 1, где Q также простое.",
                nameof(p));
        }
    }

    private void ValidatePrimitiveRoot(BigInteger p, BigInteger q, BigInteger g)
    {
        if (g <= 1 || g >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(g),
                "g должно удовлетворять условию 1 < g < P.");
        }

        var gSquared = _modularExponentiation.Calculate(g, 2, p);
        var gToQ = _modularExponentiation.Calculate(g, q, p);

        if (gSquared == 1 || gToQ == 1)
        {
            throw new ArgumentException(
                "g не является первообразным корнем по модулю P.",
                nameof(g));
        }
    }
}
