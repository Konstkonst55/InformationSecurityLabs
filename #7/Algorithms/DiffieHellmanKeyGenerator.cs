using System.Numerics;
using _1;

namespace _7;

public sealed class DiffieHellmanKeyGenerator
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;

    public DiffieHellmanKeyGenerator(
        IModularExponentiation modularExponentiation,
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest)
    {
        _modularExponentiation = modularExponentiation;
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
    }

    public DiffieHellmanKeyResult Generate(int bitLength = 32)
    {
        if (bitLength < 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bitLength),
                "Разрядность p должна быть не меньше 10.");
        }

        while (true)
        {
            var q = _randomNumberGenerator.GeneratePrime(
                bitLength - 1,
                _primalityTest,
                30);
            var p = 2 * q + 1;

            if (!_primalityTest.IsProbablyPrime(p, 30))
            {
                continue;
            }

            var g = FindPrimitiveRoot(p, q);
            var xa = _randomNumberGenerator.Generate(2, p - 2);
            var xb = _randomNumberGenerator.Generate(2, p - 2);

            return Calculate(p, g, xa, xb);
        }
    }

    public DiffieHellmanKeyResult Calculate(
        BigInteger p,
        BigInteger g,
        BigInteger xa,
        BigInteger xb)
    {
        ValidatePrime(p);
        ValidateGenerator(p, g);
        ValidatePrivateKey(xa, p, nameof(xa));
        ValidatePrivateKey(xb, p, nameof(xb));

        var ya = _modularExponentiation.Calculate(g, xa, p);
        var yb = _modularExponentiation.Calculate(g, xb, p);
        var ka = _modularExponentiation.Calculate(yb, xa, p);
        var kb = _modularExponentiation.Calculate(ya, xb, p);

        if (ka != kb)
        {
            throw new InvalidOperationException(
                "Секретные ключи Диффи—Хеллмана не совпали.");
        }

        return new DiffieHellmanKeyResult(
            p,
            g,
            xa,
            xb,
            ya,
            yb,
            ka,
            kb);
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

        throw new InvalidOperationException(
            "Не удалось найти первообразный корень.");
    }

    private void ValidatePrime(BigInteger p)
    {
        if (p <= 257 || !_primalityTest.IsProbablyPrime(p, 30))
        {
            throw new ArgumentException(
                "p должно быть простым числом больше 257.",
                nameof(p));
        }

        var q = (p - 1) / 2;
        if (2 * q + 1 != p || !_primalityTest.IsProbablyPrime(q, 30))
        {
            throw new ArgumentException(
                "p должно быть безопасным простым числом вида p = 2q + 1.",
                nameof(p));
        }
    }

    private void ValidateGenerator(BigInteger p, BigInteger g)
    {
        if (g <= 1 || g >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(g),
                "g должно удовлетворять 1 < g < p.");
        }

        var q = (p - 1) / 2;
        var gSquared = _modularExponentiation.Calculate(g, 2, p);
        var gToQ = _modularExponentiation.Calculate(g, q, p);

        if (gSquared == 1 || gToQ == 1)
        {
            throw new ArgumentException(
                "g не является первообразным корнем для выбранного p.",
                nameof(g));
        }
    }

    private static void ValidatePrivateKey(
        BigInteger x,
        BigInteger p,
        string name)
    {
        if (x < 2 || x > p - 2)
        {
            throw new ArgumentOutOfRangeException(
                name,
                "Секретный показатель должен удовлетворять 2 <= x <= p - 2.");
        }
    }
}
