using System.Numerics;
using _1;

namespace _8;

public sealed class ConsoleUi
{
    private readonly RsaSignatureAlgorithm _algorithm;
    private readonly RsaSignatureFileService _fileService;
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public ConsoleUi(
        RsaSignatureAlgorithm algorithm,
        RsaSignatureFileService fileService,
        IRandomNumberGenerator randomNumberGenerator)
    {
        _algorithm = algorithm;
        _fileService = fileService;
        _randomNumberGenerator = randomNumberGenerator;
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
                        SignFile();
                        break;
                    case "2":
                        VerifyFile();
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
                IOException or
                InvalidOperationException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void SignFile()
    {
        var keys = ReadKeysForSigning();
        PrintKeys(keys);

        var inputPath = ReadText("Файл для подписания: ");
        var defaultSignaturePath = inputPath + ".sig.json";
        var signaturePath = ReadOptionalText(
            $"Файл подписи [{defaultSignaturePath}]: ",
            defaultSignaturePath);

        var result = _fileService.SignFile(inputPath, signaturePath, keys);
        Console.WriteLine($"SHA-256: {result.HashHex}");
        Console.WriteLine($"Подписано байт хеша: {result.SignedByteCount}");
        Console.WriteLine($"Подпись сохранена: {result.SignaturePath}");
    }

    private void VerifyFile()
    {
        var inputPath = ReadText("Файл для проверки: ");
        var signaturePath = ReadText("Файл подписи: ");
        var isValid = _fileService.VerifyFile(inputPath, signaturePath);

        Console.WriteLine(isValid
            ? "Подпись корректна. Файл не изменён после подписания."
            : "Подпись некорректна или файл был изменён.");
    }

    private void GenerateAndPrintKeys()
    {
        var keys = _algorithm.GenerateKeys(
            ReadInt("Разрядность P и Q: ", 32),
            _randomNumberGenerator);

        PrintKeys(keys);
    }

    private RsaSignatureKeys ReadKeysForSigning()
    {
        Console.WriteLine("1 - ввод P, Q, d");
        Console.WriteLine("2 - генерация P, Q, d, c и других параметров");
        Console.Write("Режим: ");

        return Console.ReadLine() switch
        {
            "1" => _algorithm.CreateKeys(
                ReadBigInteger("P: "),
                ReadBigInteger("Q: "),
                ReadBigInteger("d: ")),
            "2" => _algorithm.GenerateKeys(
                ReadInt("Разрядность P и Q: ", 32),
                _randomNumberGenerator),
            _ => throw new ArgumentException("Неизвестный режим.")
        };
    }

    private static void PrintKeys(RsaSignatureKeys keys)
    {
        Console.WriteLine($"P: {keys.P}");
        Console.WriteLine($"Q: {keys.Q}");
        Console.WriteLine($"N: {keys.N}");
        Console.WriteLine($"φ: {keys.Phi}");
        Console.WriteLine($"d (открытый показатель): {keys.D}");
        Console.WriteLine($"c (закрытый показатель): {keys.C}");
        Console.WriteLine($"Проверка c * d mod φ: {(keys.C * keys.D) % keys.Phi}");
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

    private static string ReadOptionalText(string prompt, string defaultValue)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
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
        Console.WriteLine("1 - Подписать файл");
        Console.WriteLine("2 - Проверить подпись файла");
        Console.WriteLine("3 - Сгенерировать параметры RSA");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
