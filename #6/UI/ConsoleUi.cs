using System.Numerics;
using Shared;

namespace _6;

public sealed class ConsoleUi
{
    private readonly RsaAlgorithm _algorithm;
    private readonly _1.IRandomNumberGenerator _randomNumberGenerator;
    private readonly _1.IPrimalityTest _primalityTest;
    private readonly IFileProcessingService _fileProcessingService;

    public ConsoleUi(
        RsaAlgorithm algorithm,
        _1.IRandomNumberGenerator randomNumberGenerator,
        _1.IPrimalityTest primalityTest,
        IFileProcessingService fileProcessingService)
    {
        _algorithm = algorithm;
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _fileProcessingService = fileProcessingService;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            var command = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (command)
                {
                    case "1":
                        Encrypt();
                        break;
                    case "2":
                        Decrypt();
                        break;
                    case "3":
                        GenerateAndPrintParameters();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException or
                FormatException or
                OverflowException or
                InvalidDataException or
                FileNotFoundException or
                InvalidOperationException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void Encrypt()
    {
        var parameters = ReadParametersForEncryption();
        PrintParameters(parameters);

        var inputPath = ReadText("Файл для шифрования: ");
        var outputPath = ReadText("Файл с шифротекстом: ");
        var blockWidth = GetBlockWidth(parameters.N);

        var result = _fileProcessingService.Encrypt(
            inputPath,
            outputPath,
            blockWidth,
            1,
            message => new[] { _algorithm.EncryptBlock(message, parameters) });

        Console.WriteLine($"Зашифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Decrypt()
    {
        var parameters = ReadParametersForDecryption();
        PrintParameters(parameters);

        var inputPath = ReadText("Файл с шифротекстом: ");
        var outputPath = ReadText("Файл после расшифрования: ");
        var blockWidth = GetBlockWidth(parameters.N);

        var result = _fileProcessingService.Decrypt(
            inputPath,
            outputPath,
            blockWidth,
            1,
            values =>
            {
                var message = _algorithm.DecryptBlock(values[0], parameters);
                if (message > byte.MaxValue)
                {
                    throw new InvalidDataException(
                        "Расшифрованный блок не является байтом.");
                }

                return (byte)message;
            });

        Console.WriteLine($"Расшифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void GenerateAndPrintParameters()
    {
        var parameters = _algorithm.GenerateParameters(
            ReadInt("Разрядность P и Q: ", 32),
            _randomNumberGenerator,
            _primalityTest);

        PrintParameters(parameters);
    }

    private RsaParameters ReadParametersForEncryption()
    {
        Console.WriteLine("1 - ввод P, Q, d");
        Console.WriteLine("2 - генерация P, Q, d, c и других параметров");
        Console.Write("Режим: ");

        return Console.ReadLine() switch
        {
            "1" => _algorithm.CreateParameters(
                ReadBigInteger("P: "),
                ReadBigInteger("Q: "),
                ReadBigInteger("d: "),
                _primalityTest),
            "2" => _algorithm.GenerateParameters(
                ReadInt("Разрядность P и Q: ", 32),
                _randomNumberGenerator,
                _primalityTest),
            _ => throw new ArgumentException("Неизвестный режим.")
        };
    }

    private RsaParameters ReadParametersForDecryption()
    {
        Console.WriteLine("1 - ввод P, Q, c");
        Console.WriteLine("2 - ввод P, Q, d и вычисление c");
        Console.Write("Режим: ");

        if (Console.ReadLine() == "1")
        {
            var p = ReadBigInteger("P: ");
            var q = ReadBigInteger("Q: ");
            var c = ReadBigInteger("c: ");
            return _algorithm.CreateParametersWithC(
                p,
                q,
                c,
                _primalityTest);
        }

        return _algorithm.CreateParameters(
            ReadBigInteger("P: "),
            ReadBigInteger("Q: "),
            ReadBigInteger("d: "),
            _primalityTest);
    }

    private static void PrintParameters(RsaParameters parameters)
    {
        Console.WriteLine($"P: {parameters.P}");
        Console.WriteLine($"Q: {parameters.Q}");
        Console.WriteLine($"N: {parameters.N}");
        Console.WriteLine($"phi: {parameters.Phi}");
        Console.WriteLine($"d: {parameters.D}");
        Console.WriteLine($"c: {parameters.C}");
        Console.WriteLine(
            $"gcd(d, phi): {_gcd(parameters.D, parameters.Phi)}");
        Console.WriteLine(
            $"c * d mod phi: {(parameters.C * parameters.D) % parameters.Phi}");
    }

    private static BigInteger _gcd(BigInteger a, BigInteger b)
    {
        while (b != 0)
        {
            var remainder = a % b;
            a = b;
            b = remainder;
        }

        return BigInteger.Abs(a);
    }

    private static int GetBlockWidth(BigInteger n)
    {
        if (n <= byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(n),
                "N должно быть больше 255.");
        }

        return n.ToByteArray(
            isUnsigned: true,
            isBigEndian: true).Length;
    }

    private static string ReadText(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("Строка не должна быть пустой.");
        }

        return value;
    }

    private static BigInteger ReadBigInteger(string prompt)
    {
        Console.Write(prompt);
        if (!BigInteger.TryParse(Console.ReadLine(), out var result))
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

    private static void PrintMenu()
    {
        Console.WriteLine("1 - Шифрование файла");
        Console.WriteLine("2 - Расшифрование файла");
        Console.WriteLine("3 - Сгенерировать параметры RSA");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
