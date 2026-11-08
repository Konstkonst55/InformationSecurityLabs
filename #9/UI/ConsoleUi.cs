using System.Numerics;

namespace _9;

public sealed class ConsoleUi
{
    private readonly ElGamalSignatureAlgorithm _algorithm;
    private readonly ElGamalSignatureKeyGenerator _keyGenerator;
    private readonly ElGamalSignatureFileService _fileService;

    public ConsoleUi(
        ElGamalSignatureAlgorithm algorithm,
        ElGamalSignatureKeyGenerator keyGenerator,
        ElGamalSignatureFileService fileService)
    {
        _algorithm = algorithm;
        _keyGenerator = keyGenerator;
        _fileService = fileService;
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
        var keyPair = ReadKeyPairForSigning();
        PrintKeys(keyPair);

        var inputPath = ReadText("Файл для подписания: ");
        var defaultSignaturePath = inputPath + ".sig.json";
        var signaturePath = ReadOptionalText(
            $"Файл подписи [{defaultSignaturePath}]: ",
            defaultSignaturePath);

        var result = _fileService.SignFile(inputPath, signaturePath, keyPair);
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
        var keyPair = _keyGenerator.Generate(
            ReadInt("Разрядность P: ", 32));
        PrintKeys(keyPair);
    }

    private ElGamalSignatureKeyPair ReadKeyPairForSigning()
    {
        Console.WriteLine("1 - ввод P, g, x");
        Console.WriteLine("2 - генерация P, Q, g, x, y");
        Console.Write("Режим: ");

        return Console.ReadLine() switch
        {
            "1" => _keyGenerator.Create(
                ReadBigInteger("P: "),
                ReadBigInteger("g: "),
                ReadBigInteger("x (секретный ключ): ")),
            "2" => _keyGenerator.Generate(
                ReadInt("Разрядность P: ", 32)),
            _ => throw new ArgumentException("Неизвестный режим.")
        };
    }

    private static void PrintKeys(ElGamalSignatureKeyPair keyPair)
    {
        var publicKey = keyPair.PublicKey;
        Console.WriteLine($"P: {publicKey.P}");
        Console.WriteLine($"Q: {publicKey.Q}");
        Console.WriteLine($"g: {publicKey.G}");
        Console.WriteLine($"x (секретный ключ): {keyPair.X}");
        Console.WriteLine($"y (открытый ключ): {publicKey.Y}");
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
        Console.WriteLine("3 - Сгенерировать параметры Эль-Гамаля");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
