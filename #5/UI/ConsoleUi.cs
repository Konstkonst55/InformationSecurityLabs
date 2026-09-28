using System.Numerics;
using Shared;
using _1;

namespace _5;

public sealed class ConsoleUi
{
    private readonly ElGamalAlgorithm _algorithm;
    private readonly ElGamalParameterGenerator _parameterGenerator;
    private readonly IFileProcessingService _fileProcessingService;

    public ConsoleUi(
        ElGamalAlgorithm algorithm,
        ElGamalParameterGenerator parameterGenerator,
        IFileProcessingService fileProcessingService)
    {
        _algorithm = algorithm;
        _parameterGenerator = parameterGenerator;
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
        var publicParameters = ReadPublicParametersForEncryption();
        PrintPublicParameters(publicParameters);

        var inputPath = ReadText("Файл для шифрования: ");
        var outputPath = ReadText("Файл с шифротекстом: ");
        var blockWidth = GetBlockWidth(publicParameters.P);

        var result = _fileProcessingService.Encrypt(
            inputPath,
            outputPath,
            blockWidth,
            2,
            message => _algorithm.EncryptBlock(message, publicParameters));

        Console.WriteLine($"Зашифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Decrypt()
    {
        var p = ReadBigInteger("p: ");
        var db = ReadBigInteger("Db: ");
        var privateParameters = _parameterGenerator.CreatePrivateParameters(p, db);

        var inputPath = ReadText("Файл с шифротекстом: ");
        var outputPath = ReadText("Файл после расшифрования: ");
        var blockWidth = GetBlockWidth(privateParameters.P);

        var result = _fileProcessingService.Decrypt(
            inputPath,
            outputPath,
            blockWidth,
            2,
            values =>
            {
                var message = _algorithm.DecryptBlock(values, privateParameters);
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
        var keyPair = _parameterGenerator.Generate(
            ReadInt("Разрядность p: ", 32));

        PrintPublicParameters(keyPair.PublicParameters);
        Console.WriteLine($"Db: {keyPair.PrivateParameters.Db}");
    }

    private ElGamalPublicParameters ReadPublicParametersForEncryption()
    {
        Console.WriteLine("1 - ввод p, g, Cb");
        Console.WriteLine("2 - генерация p, g, Cb, Db");
        Console.Write("Режим: ");

        var mode = Console.ReadLine();

        if (mode == "1")
        {
            return _parameterGenerator.CreatePublicParameters(
                ReadBigInteger("p: "),
                ReadBigInteger("g: "),
                ReadBigInteger("Cb: "));
        }

        if (mode == "2")
        {
            var keyPair = _parameterGenerator.Generate(
                ReadInt("Разрядность p: ", 32));
            Console.WriteLine($"Db: {keyPair.PrivateParameters.Db}");
            return keyPair.PublicParameters;
        }

        throw new ArgumentException("Неизвестный режим.");
    }

    private static void PrintPublicParameters(
        ElGamalPublicParameters parameters)
    {
        Console.WriteLine($"p: {parameters.P}");
        Console.WriteLine($"g: {parameters.G}");
        Console.WriteLine($"Cb: {parameters.Cb}");
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
        Console.WriteLine("3 - Сгенерировать параметры Эль-Гамаля");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
