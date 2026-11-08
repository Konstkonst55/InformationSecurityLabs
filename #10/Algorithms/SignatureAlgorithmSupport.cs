using System.Globalization;
using System.Numerics;
using _1;

namespace _10.Algorithms;

internal static class SignatureAlgorithmSupport
{
    public static BigInteger Normalize(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;
        return result.Sign < 0 ? result + modulus : result;
    }

    public static BigInteger ModInverse(
        BigInteger value,
        BigInteger modulus,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm)
    {
        var result = extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != BigInteger.One)
        {
            throw new ArithmeticException("Обратный элемент по заданному модулю не существует.");
        }

        return Normalize(result.X, modulus);
    }

    public static BigInteger ReadParameter(IReadOnlyDictionary<string, string> values, string name)
    {
        if (!values.TryGetValue(name, out var raw) ||
            !BigInteger.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new InvalidDataException($"В параметрах отсутствует корректное значение {name}.");
        }

        return value;
    }

    public static void ValidateDomainParameters(
        BigInteger p,
        BigInteger q,
        BigInteger generator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        if (p <= q || q <= 2 || !primalityTest.IsProbablyPrime(q, 40) ||
            !primalityTest.IsProbablyPrime(p, 40) || (p - BigInteger.One) % q != BigInteger.Zero)
        {
            throw new InvalidDataException("Параметры должны удовлетворять условиям: p и q простые, q делит p - 1.");
        }

        if (generator <= BigInteger.One || generator >= p ||
            modularExponentiation.Calculate(generator, q, p) != BigInteger.One)
        {
            throw new InvalidDataException("Параметр a/g не принадлежит подгруппе порядка q.");
        }
    }

    public static void ValidatePublicKey(
        BigInteger p,
        BigInteger q,
        BigInteger generator,
        BigInteger y,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation)
    {
        ValidateDomainParameters(p, q, generator, primalityTest, modularExponentiation);
        if (y <= BigInteger.One || y >= p ||
            modularExponentiation.Calculate(y, q, p) != BigInteger.One)
        {
            throw new InvalidDataException("Открытый ключ y не принадлежит подгруппе порядка q.");
        }
    }
}
