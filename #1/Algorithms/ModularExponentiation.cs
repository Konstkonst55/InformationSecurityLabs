using System.Numerics;

namespace _1;

public sealed class ModularExponentiation : IModularExponentiation
{
    public BigInteger Calculate(BigInteger a, BigInteger exponent, BigInteger modulus)
    {
        if (modulus <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modulus), "Модуль должен быть положительным.");
        }

        if (exponent < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exponent), "Показатель степени не может быть отрицательным.");
        }

        var result = BigInteger.One % modulus;
        var baseValue = Normalize(a, modulus);
        var power = exponent;

        while (power > 0)
        {
            if (!power.IsEven)
            {
                result = result * baseValue % modulus;
            }

            baseValue = baseValue * baseValue % modulus;
            power >>= 1;
        }

        return result;
    }

    private static BigInteger Normalize(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
