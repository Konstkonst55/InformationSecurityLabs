using System.Numerics;

namespace _1;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation modularExponentiation = new ModularExponentiation();
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator randomNumberGenerator = new SecureRandomNumberGenerator();
        IPrimalityTest primalityTest = new FermatPrimalityTest(
            extendedEuclideanAlgorithm,
            modularExponentiation,
            randomNumberGenerator);

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
                        RunModularExponentiation(modularExponentiation);
                        break;
                    case "2":
                        RunFermat(primalityTest);
                        break;
                    case "3":
                        RunExtendedEuclid(extendedEuclideanAlgorithm, primalityTest, randomNumberGenerator);
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

    private static void RunModularExponentiation(IModularExponentiation algorithm)
    {
        var a = ReadBigInteger("a: ");
        var exponent = ReadBigInteger("n: ");
        var modulus = ReadBigInteger("p: ");
        Console.WriteLine(algorithm.Calculate(a, exponent, modulus));
    }

    private static void RunFermat(IPrimalityTest algorithm)
    {
        var number = ReadBigInteger("p: ");
        var iterations = ReadInt("K: ", 100);
        Console.WriteLine(algorithm.IsProbablyPrime(number, iterations) ? "Вероятно простое" : "Составное");
    }

    private static void RunExtendedEuclid(
        IExtendedEuclideanAlgorithm algorithm,
        IPrimalityTest primalityTest,
        IRandomNumberGenerator randomNumberGenerator)
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
                a = randomNumberGenerator.GeneratePositive(bits);
                b = randomNumberGenerator.GeneratePositive(bits);
                break;
            case "3":
                var primeBits = ReadInt("bits: ", 32);
                a = randomNumberGenerator.GeneratePrime(primeBits, primalityTest);
                do
                {
                    b = randomNumberGenerator.GeneratePrime(primeBits, primalityTest);
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

        var result = algorithm.Calculate(a, b);
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
