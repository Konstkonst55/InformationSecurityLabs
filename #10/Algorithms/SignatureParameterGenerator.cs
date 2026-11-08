using System.Numerics;
using _1;
using _10.Models;

namespace _10.Algorithms;

public sealed class SignatureParameterGenerator
{
    public const string GostAlgorithmName = "GOST-R-34.10-94";
    public const string DsaAlgorithmName = "DSA-FIPS-186-HISTORICAL";

    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IModularExponentiation _modularExponentiation;

    public SignatureParameterGenerator(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _modularExponentiation = modularExponentiation;
    }

    public SignatureKeyPair Generate(string algorithm, int pBitLength)
    {
        var generatorParameterName = GetGeneratorParameterName(algorithm);
        var maximumQBitLength = algorithm == GostAlgorithmName ? 16 : 160;
        var qBitLength = Math.Min(maximumQBitLength, (pBitLength + 1) / 2);
        ValidateBitLengths(pBitLength, qBitLength);

        var (p, q, generator) = GenerateDomainParameters(pBitLength, qBitLength);
        var x = _randomNumberGenerator.Generate(BigInteger.One, q - BigInteger.One);
        var y = _modularExponentiation.Calculate(generator, x, p);

        return new SignatureKeyPair(algorithm, generatorParameterName, p, q, generator, x, y);
    }

    public SignatureKeyPair Create(
        string algorithm,
        BigInteger p,
        BigInteger q,
        BigInteger generator,
        BigInteger x)
    {
        var generatorParameterName = GetGeneratorParameterName(algorithm);
        SignatureAlgorithmSupport.ValidateDomainParameters(
            p,
            q,
            generator,
            _primalityTest,
            _modularExponentiation);

        if (x <= BigInteger.Zero || x >= q)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Секретный ключ x должен удовлетворять условию 0 < x < q.");
        }

        var y = _modularExponentiation.Calculate(generator, x, p);
        return new SignatureKeyPair(algorithm, generatorParameterName, p, q, generator, x, y);
    }

    private (BigInteger P, BigInteger Q, BigInteger Generator) GenerateDomainParameters(
        int pBitLength,
        int qBitLength)
    {
        var minimumP = BigInteger.One << (pBitLength - 1);
        var maximumP = (BigInteger.One << pBitLength) - BigInteger.One;

        while (true)
        {
            var q = _randomNumberGenerator.GeneratePrime(qBitLength, _primalityTest, 40);
            var minimumB = CeilingDivide(minimumP - BigInteger.One, q);
            var maximumB = (maximumP - BigInteger.One) / q;

            if (!minimumB.IsEven)
            {
                minimumB++;
            }

            if (!maximumB.IsEven)
            {
                maximumB--;
            }

            if (minimumB > maximumB)
            {
                continue;
            }

            var minimumHalf = minimumB / 2;
            var maximumHalf = maximumB / 2;

            for (var attempt = 0; attempt < 4096; attempt++)
            {
                var b = _randomNumberGenerator.Generate(minimumHalf, maximumHalf) * 2;
                var p = b * q + BigInteger.One;

                if (p.GetBitLength() != pBitLength ||
                    !_primalityTest.IsProbablyPrime(p, 40))
                {
                    continue;
                }

                for (var generatorAttempt = 0; generatorAttempt < 128; generatorAttempt++)
                {
                    var value = _randomNumberGenerator.Generate(2, p - 2);
                    var generator = _modularExponentiation.Calculate(value, b, p);
                    if (generator > BigInteger.One)
                    {
                        return (p, q, generator);
                    }
                }
            }
        }
    }

    private static BigInteger CeilingDivide(BigInteger value, BigInteger divisor)
    {
        return (value + divisor - BigInteger.One) / divisor;
    }

    private static void ValidateBitLengths(int pBitLength, int qBitLength)
    {
        if (qBitLength < 3 || pBitLength < qBitLength + 2 || pBitLength > 4096)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pBitLength),
                "Требуется 3 <= разрядность Q <= разрядность P - 2; максимальная разрядность P — 4096 бит.");
        }
    }

    private static string GetGeneratorParameterName(string algorithm)
    {
        return algorithm switch
        {
            GostAlgorithmName => "A",
            DsaAlgorithmName => "G",
            _ => throw new ArgumentException("Неизвестный алгоритм подписи.", nameof(algorithm))
        };
    }
}
