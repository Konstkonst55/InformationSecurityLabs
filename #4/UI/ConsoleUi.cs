using System.Numerics;
using Shared;
using _1;

namespace _4;

public sealed class ConsoleUi
{
    private readonly ShamirAlgorithm _algorithm;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IExtendedEuclideanAlgorithm _extendedEuclideanAlgorithm;
    private readonly IFileProcessingService _fileProcessingService;

    public ConsoleUi(
        ShamirAlgorithm algorithm,
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IExtendedEuclideanAlgorithm extendedEuclideanAlgorithm,
        IFileProcessingService fileProcessingService)
    {
        _algorithm = algorithm;
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _extendedEuclideanAlgorithm = extendedEuclideanAlgorithm;
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
                        GenerateAndPrintKeys();
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
        var keys = ReadKeysForEncryption();
        PrintKeys(keys);

        var inputPath = ReadText("Файл для шифрования: ");
        var outputPath = ReadText("Файл с шифротекстом: ");
        var blockWidth = GetBlockWidth(keys.P);

        var result = _fileProcessingService.Encrypt(
            inputPath,
            outputPath,
            blockWidth,
            1,
            message => new[] { _algorithm.EncryptBlock(message, keys) });

        Console.WriteLine($"Зашифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Decrypt()
    {
        var p = ReadBigInteger("p: ");
        var da = ReadBigInteger("Da: ");
        var db = ReadBigInteger("Db: ");
        var keys = new ShamirKeys(p, 0, da, 0, db);

        var inputPath = ReadText("Файл с шифротекстом: ");
        var outputPath = ReadText("Файл после расшифрования: ");
        var blockWidth = GetBlockWidth(keys.P);

        var result = _fileProcessingService.Decrypt(
            inputPath,
            outputPath,
            blockWidth,
            1,
            values =>
            {
                var message = _algorithm.DecryptBlock(values[0], keys);
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

    private void GenerateAndPrintKeys()
    {
        var keys = _algorithm.GenerateKeys(
            _randomNumberGenerator,
            _primalityTest,
            _extendedEuclideanAlgorithm,
            ReadInt("Разрядность p: ", 32));

        PrintKeys(keys);
    }

    private ShamirKeys ReadKeysForEncryption()
    {
        Console.WriteLine("1 - ввод p, Ca, Cb");
        Console.WriteLine("2 - генерация p, Ca, Cb, Da, Db");
        Console.Write("Режим: ");

        var mode = Console.ReadLine();

        return mode switch
        {
            "1" => _algorithm.CreateKeys(
                ReadBigInteger("p: "),
                ReadBigInteger("Ca: "),
                ReadBigInteger("Cb: "),
                _primalityTest,
                _extendedEuclideanAlgorithm),
            "2" => _algorithm.GenerateKeys(
                _randomNumberGenerator,
                _primalityTest,
                _extendedEuclideanAlgorithm,
                ReadInt("Разрядность p: ", 32)),
            _ => throw new ArgumentException("Неизвестный режим.")
        };
    }

    private static void PrintKeys(ShamirKeys keys)
    {
        Console.WriteLine($"p: {keys.P}");
        Console.WriteLine($"Ca: {keys.Ca}");
        Console.WriteLine($"Da: {keys.Da}");
        Console.WriteLine($"Cb: {keys.Cb}");
        Console.WriteLine($"Db: {keys.Db}");
        Console.WriteLine(
            $"Ca * Da mod (p - 1): {(keys.Ca * keys.Da) % (keys.P - 1)}");
        Console.WriteLine(
            $"Cb * Db mod (p - 1): {(keys.Cb * keys.Db) % (keys.P - 1)}");
    }

    private static int GetBlockWidth(BigInteger p)
    {
        if (p <= byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(p),
                "p должно быть больше 255.");
        }

        return p.ToByteArray(
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
        Console.WriteLine("3 - Сгенерировать параметры Шамира");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
