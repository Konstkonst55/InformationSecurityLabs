using System.Numerics;
using _1;

namespace _6;

public sealed class RsaAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;

    public RsaAlgorithm(
        IModularExponentiation modularExponentiation,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        _modularExponentiation = modularExponentiation;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
    }

    public RsaParameters CreateParameters(
        BigInteger p,
        BigInteger q,
        BigInteger d,
        IPrimalityTest primalityTest)
    {
        ValidatePrime(p, nameof(p), primalityTest);
        ValidatePrime(q, nameof(q), primalityTest);

        if (p == q)
        {
            throw new ArgumentException("P и Q должны быть различными простыми числами.");
        }

        var n = p * q;
        var phi = (p - 1) * (q - 1);

        ValidateD(d, phi);
        var c = Inverse(d, phi);

        return new RsaParameters(p, q, n, phi, d, c);
    }

    public RsaParameters CreateParametersWithC(
        BigInteger p,
        BigInteger q,
        BigInteger c,
        IPrimalityTest primalityTest)
    {
        ValidatePrime(p, nameof(p), primalityTest);
        ValidatePrime(q, nameof(q), primalityTest);

        if (p == q)
        {
            throw new ArgumentException("P и Q должны быть различными простыми числами.");
        }

        var n = p * q;
        var phi = (p - 1) * (q - 1);

        if (c <= 1 || c >= phi)
        {
            throw new ArgumentOutOfRangeException(
                nameof(c),
                "c должно удовлетворять условию 1 < c < phi.");
        }

        if (_extendedEuclideanAlgorithm.Calculate(c, phi).Gcd != 1)
        {
            throw new ArgumentException(
                "c должно быть взаимно простым с phi.",
                nameof(c));
        }

        var d = Inverse(c, phi);
        return new RsaParameters(p, q, n, phi, d, c);
    }

    public BigInteger EncryptBlock(
        BigInteger m,
        RsaParameters parameters)
    {
        ValidateMessage(m, parameters.N);
        return _modularExponentiation.Calculate(
            m,
            parameters.D,
            parameters.N);
    }

    public BigInteger DecryptBlock(
        BigInteger e,
        RsaParameters parameters)
    {
        if (e < 0 || e >= parameters.N)
        {
            throw new InvalidDataException(
                "Значение криптограммы должно удовлетворять 0 <= e < N.");
        }

        return _modularExponentiation.Calculate(
            e,
            parameters.C,
            parameters.N);
    }

    public RsaParameters GenerateParameters(
        int bitLength,
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest)
    {
        if (bitLength < 16)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bitLength),
                "Разрядность простых чисел должна быть не меньше 16.");
        }

        while (true)
        {
            var p = randomNumberGenerator.GeneratePrime(
                bitLength,
                primalityTest,
                20);
            var q = randomNumberGenerator.GeneratePrime(
                bitLength,
                primalityTest,
                20);

            if (p == q)
            {
                continue;
            }

            var phi = (p - 1) * (q - 1);
            var d = GenerateD(phi, randomNumberGenerator);
            var c = Inverse(d, phi);

            return new RsaParameters(
                p,
                q,
                p * q,
                phi,
                d,
                c);
        }
    }

    private BigInteger GenerateD(
        BigInteger phi,
        IRandomNumberGenerator randomNumberGenerator)
    {
        while (true)
        {
            var d = randomNumberGenerator.Generate(2, phi - 1);
            if (_extendedEuclideanAlgorithm.Calculate(d, phi).Gcd == 1)
            {
                return d;
            }
        }
    }

    private BigInteger Inverse(
        BigInteger value,
        BigInteger modulus)
    {
        var result = _extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException(
                "Для d не существует обратного элемента по модулю phi.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }

    private static void ValidatePrime(
        BigInteger value,
        string name,
        IPrimalityTest primalityTest)
    {
        if (value <= 257 || !primalityTest.IsProbablyPrime(value, 20))
        {
            throw new ArgumentException(
                "P и Q должны быть простыми числами больше 257.",
                name);
        }
    }

    private static void ValidateD(
        BigInteger d,
        BigInteger phi)
    {
        if (d <= 1 || d >= phi)
        {
            throw new ArgumentOutOfRangeException(
                nameof(d),
                "d должно удовлетворять условию 1 < d < phi.");
        }
    }

    private void ValidateMessage(
        BigInteger m,
        BigInteger n)
    {
        if (m < 0 || m >= n)
        {
            throw new ArgumentOutOfRangeException(
                nameof(m),
                "Сообщение должно удовлетворять условию 0 <= m < N.");
        }
    }
}
