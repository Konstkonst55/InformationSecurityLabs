using System.Numerics;
using _1;

namespace _5;

public sealed class ElGamalParameterGenerator
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IModularExponentiation _modularExponentiation;

    public ElGamalParameterGenerator(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _modularExponentiation = modularExponentiation;
    }

    public ElGamalKeyPair Generate(
        int bitLength = 32,
        int primalityIterations = 20)
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
                primalityIterations);
            var p = 2 * q + 1;

            if (!_primalityTest.IsProbablyPrime(p, primalityIterations))
            {
                continue;
            }

            var g = FindPrimitiveRoot(p, q);
            var db = _randomNumberGenerator.Generate(2, p - 2);
            var cb = _modularExponentiation.Calculate(g, db, p);

            return new ElGamalKeyPair(
                new ElGamalPublicParameters(p, g, cb),
                new ElGamalPrivateParameters(p, db));
        }
    }

    public ElGamalPublicParameters CreatePublicParameters(
        BigInteger p,
        BigInteger g,
        BigInteger cb)
    {
        ValidateModulus(p);
        ValidatePrimitiveRoot(p, g);

        if (cb <= 0 || cb >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cb),
                "Cb должно удовлетворять 0 < Cb < p.");
        }

        return new ElGamalPublicParameters(p, g, cb);
    }

    public ElGamalPrivateParameters CreatePrivateParameters(
        BigInteger p,
        BigInteger db)
    {
        ValidateModulus(p);

        if (db <= 1 || db >= p - 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(db),
                "Db должно удовлетворять 1 < Db < p - 1.");
        }

        return new ElGamalPrivateParameters(p, db);
    }

    private BigInteger FindPrimitiveRoot(
        BigInteger p,
        BigInteger q)
    {
        for (var g = new BigInteger(2); g < p - 1; g++)
        {
            var first = _modularExponentiation.Calculate(g, 2, p);
            var second = _modularExponentiation.Calculate(g, q, p);

            if (first != 1 && second != 1)
            {
                return g;
            }
        }

        throw new InvalidOperationException(
            "Не удалось найти первообразный корень.");
    }

    private void ValidateModulus(BigInteger p)
    {
        if (p <= 257 || !_primalityTest.IsProbablyPrime(p, 20))
        {
            throw new ArgumentException(
                "p должно быть простым числом больше 257.",
                nameof(p));
        }

        var q = (p - 1) / 2;
        if (2 * q + 1 != p || !_primalityTest.IsProbablyPrime(q, 20))
        {
            throw new ArgumentException(
                "p должно быть безопасным простым числом вида 2q + 1.",
                nameof(p));
        }
    }

    private void ValidatePrimitiveRoot(
        BigInteger p,
        BigInteger g)
    {
        if (g <= 1 || g >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(g),
                "g должно удовлетворять 1 < g < p.");
        }

        var q = (p - 1) / 2;
        var first = _modularExponentiation.Calculate(g, 2, p);
        var second = _modularExponentiation.Calculate(g, q, p);

        if (first == 1 || second == 1)
        {
            throw new ArgumentException(
                "g не является первообразным корнем для выбранного p.",
                nameof(g));
        }
    }
}
