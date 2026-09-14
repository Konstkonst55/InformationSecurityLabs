using System.Numerics;
using _1;

namespace _2;

public sealed class BabyStepGiantStepAlgorithm : IDiscreteLogarithmSolver
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IPrimalityTest _primalityTest;

    public BabyStepGiantStepAlgorithm(
        IModularExponentiation modularExponentiation,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IPrimalityTest primalityTest)
    {
        _modularExponentiation = modularExponentiation;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
        _primalityTest = primalityTest;
    }

    public DiscreteLogResult Solve(BigInteger a, BigInteger y, BigInteger p)
    {
        Validate(a, y, p);
        if (!_primalityTest.IsProbablyPrime(p, 20))
        {
            throw new ArgumentException("p должно быть простым числом.", nameof(p));
        }

        var orderBound = p - 1;
        var m = IntegerSquareRoot(orderBound);
        if (m * m < orderBound)
        {
            m++;
        }

        var babySteps = BuildBabySteps(a, p, m);
        var factor = ModularInverse(_modularExponentiation.Calculate(a, m, p), p);
        var gamma = y;

        for (var i = BigInteger.Zero; i <= m; i++)
        {
            if (babySteps.TryGetValue(gamma, out var j))
            {
                var x = i * m + j;
                if (x < orderBound && _modularExponentiation.Calculate(a, x, p) == y)
                {
                    return new DiscreteLogResult(a, y, p, m, x, true);
                }
            }

            gamma = gamma * factor % p;
        }

        return new DiscreteLogResult(a, y, p, m, null, false);
    }

    private Dictionary<BigInteger, BigInteger> BuildBabySteps(BigInteger a, BigInteger p, BigInteger m)
    {
        var result = new Dictionary<BigInteger, BigInteger>();
        var current = BigInteger.One;

        for (var j = BigInteger.Zero; j < m; j++)
        {
            result.TryAdd(current, j);
            current = current * a % p;
        }

        return result;
    }

    private BigInteger ModularInverse(BigInteger value, BigInteger modulus)
    {
        var result = _extendedEuclideanAlgorithm.Calculate(value, modulus);
        if (result.Gcd != 1)
        {
            throw new InvalidOperationException("Обратный элемент не существует.");
        }

        var inverse = result.X % modulus;
        return inverse < 0 ? inverse + modulus : inverse;
    }

    private static void Validate(BigInteger a, BigInteger y, BigInteger p)
    {
        if (p <= 2)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Модуль p должен быть простым числом больше 2.");
        }

        if (a < 1 || a >= p)
        {
            throw new ArgumentOutOfRangeException(nameof(a), "Основание a должно удовлетворять 1 <= a < p.");
        }

        if (y < 1 || y >= p)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Значение y должно удовлетворять 1 <= y < p.");
        }
    }

    private static BigInteger IntegerSquareRoot(BigInteger value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        if (value < 2)
        {
            return value;
        }

        var low = BigInteger.One;
        var high = value;

        while (low <= high)
        {
            var middle = (low + high) / 2;
            var square = middle * middle;

            if (square == value)
            {
                return middle;
            }

            if (square < value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return high;
    }
}
