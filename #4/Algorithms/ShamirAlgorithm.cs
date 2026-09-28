using System.Numerics;
using _1;

namespace _4;

public sealed class ShamirAlgorithm
{
    private readonly IModularExponentiation _modularExponentiation;

    public ShamirAlgorithm(IModularExponentiation modularExponentiation)
    {
        _modularExponentiation = modularExponentiation;
    }

    public ShamirKeys GenerateKeys(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        int bitLength = 32,
        int primalityIterations = 20)
    {
        var p = randomNumberGenerator.GeneratePrime(
            bitLength,
            primalityTest,
            primalityIterations);

        var phi = p - 1;
        var ca = GenerateExponent(phi, randomNumberGenerator, extendedEuclideanAlgorithm);
        var cb = GenerateExponent(phi, randomNumberGenerator, extendedEuclideanAlgorithm);
        var da = Inverse(ca, phi, extendedEuclideanAlgorithm);
        var db = Inverse(cb, phi, extendedEuclideanAlgorithm);

        return new ShamirKeys(p, ca, da, cb, db);
    }

    public ShamirKeys CreateKeys(
        BigInteger p,
        BigInteger ca,
        BigInteger cb,
        IPrimalityTest primalityTest,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        if (p <= 257 || !primalityTest.IsProbablyPrime(p, 20))
        {
            throw new ArgumentException(
                "p должно быть простым числом больше 257.",
                nameof(p));
        }

        var phi = p - 1;
        ValidateExponent(ca, phi, nameof(ca), extendedEuclideanAlgorithm);
        ValidateExponent(cb, phi, nameof(cb), extendedEuclideanAlgorithm);

        return new ShamirKeys(
            p,
            ca,
            Inverse(ca, phi, extendedEuclideanAlgorithm),
            cb,
            Inverse(cb, phi, extendedEuclideanAlgorithm));
    }

    public BigInteger EncryptBlock(
        BigInteger message,
        ShamirKeys keys)
    {
        ValidateMessage(message, keys.P);
        var first = _modularExponentiation.Calculate(message, keys.Ca, keys.P);
        return _modularExponentiation.Calculate(first, keys.Cb, keys.P);
    }

    public BigInteger DecryptBlock(
        BigInteger encrypted,
        ShamirKeys keys)
    {
        if (encrypted < 0 || encrypted >= keys.P)
        {
            throw new InvalidDataException("Некорректное значение блока шифротекста.");
        }

        var first = _modularExponentiation.Calculate(encrypted, keys.Da, keys.P);
        return _modularExponentiation.Calculate(first, keys.Db, keys.P);
    }

    private static BigInteger GenerateExponent(
        BigInteger phi,
        IRandomNumberGenerator randomNumberGenerator,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        while (true)
        {
            var candidate = randomNumberGenerator.Generate(2, phi - 1);
            if (extendedEuclideanAlgorithm.Calculate(candidate, phi).Gcd == 1)
            {
                return candidate;
            }
        }
    }

    private static BigInteger Inverse(
        BigInteger value,
        BigInteger modulus,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        var result = extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException("Обратный элемент не существует.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }

    private static void ValidateExponent(
        BigInteger value,
        BigInteger phi,
        string name,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        if (value <= 1 || value >= phi)
        {
            throw new ArgumentOutOfRangeException(
                name,
                "Показатель должен удовлетворять 1 < C < p - 1.");
        }

        if (extendedEuclideanAlgorithm.Calculate(value, phi).Gcd != 1)
        {
            throw new ArgumentException(
                "Показатель должен быть взаимно прост с p - 1.",
                name);
        }
    }

    private static void ValidateMessage(BigInteger message, BigInteger p)
    {
        if (message < 0 || message >= p)
        {
            throw new ArgumentOutOfRangeException(
                nameof(message),
                "Сообщение должно удовлетворять условию 0 <= m < p.");
        }
    }
}
