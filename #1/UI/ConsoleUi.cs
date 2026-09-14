using System.Numerics;

namespace _1;

public sealed class ConsoleUi
{
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IPrimalityTest _primalityTest;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public ConsoleUi(
        IModularExponentiation modularExponentiation,
        IPrimalityTest primalityTest,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IRandomNumberGenerator randomNumberGenerator)
    {
        _modularExponentiation = modularExponentiation;
        _primalityTest = primalityTest;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
        _randomNumberGenerator = randomNumberGenerator;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("1 - a^n mod p");
            Console.WriteLine("2 - Тест Ферма");
            Console.WriteLine("3 - Обобщённый алгоритм Евклида");
            Console.WriteLine("0 - Выход");
            Console.Write("Ввод: ");
            var command = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (command)
                {
                    case "1":
                        RunModularExponentiation();
                        break;
                    case "2":
                        RunFermat();
                        break;
                    case "3":
                        RunExtendedEuclid();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void RunModularExponentiation()
    {
        var a = ReadBigInteger("a: ");
        var exponent = ReadBigInteger("n: ");
        var modulus = ReadBigInteger("p: ");
        Console.WriteLine(_modularExponentiation.Calculate(a, exponent, modulus));
    }

    private void RunFermat()
    {
        var number = ReadBigInteger("p: ");
        var iterations = ReadInt("K: ", 100);
        Console.WriteLine(_primalityTest.IsProbablyPrime(number, iterations)
            ? "Вероятно простое"
            : "Составное");
    }

    private void RunExtendedEuclid()
    {
        Console.WriteLine("1 - ввод");
        Console.WriteLine("2 - случайные");
        Console.WriteLine("3 - случайные простые");
        Console.Write("Режим: ");
        var mode = Console.ReadLine();

        BigInteger a;
        BigInteger b;

        switch (mode)
        {
            case "1":
                a = ReadBigInteger("a: ");
                b = ReadBigInteger("b: ");
                break;
            case "2":
                var bits = ReadInt("bits: ", 64);
                a = _randomNumberGenerator.GeneratePositive(bits);
                b = _randomNumberGenerator.GeneratePositive(bits);
                break;
            case "3":
                var primeBits = ReadInt("bits: ", 32);
                a = _randomNumberGenerator.GeneratePrime(primeBits, _primalityTest);
                do
                {
                    b = _randomNumberGenerator.GeneratePrime(primeBits, _primalityTest);
                }
                while (a == b);
                break;
            default:
                throw new ArgumentException("Неизвестный режим.");
        }

        if (a < 0 || b < 0)
        {
            throw new ArgumentOutOfRangeException("a, b", "Для этого режима a и b должны быть неотрицательными.");
        }

        var result = _extendedEuclideanAlgorithm.Calculate(a, b);
        Console.WriteLine($"gcd: {result.Gcd}");
        Console.WriteLine($"x: {result.X}");
        Console.WriteLine($"y: {result.Y}");
        Console.WriteLine($"check: {a * result.X + b * result.Y}");
    }

    private static BigInteger ReadBigInteger(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        if (!BigInteger.TryParse(value, out var result))
        {
            throw new FormatException("Ожидалось целое число.");
        }

        return result;
    }

    private static int ReadInt(string prompt, int defaultValue)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var result))
        {
            throw new FormatException("Ожидалось целое число.");
        }

        return result;
    }
}
