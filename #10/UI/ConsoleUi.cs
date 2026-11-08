using System.Numerics;
using _10.Algorithms;
using _10.Models;
using _10.Services;

namespace _10.UI;

public sealed class ConsoleUi
{
    private readonly SignatureParameterGenerator _parameterGenerator;
    private readonly SignatureFileService _fileService;

    public ConsoleUi(
        SignatureParameterGenerator parameterGenerator,
        SignatureFileService fileService)
    {
        _parameterGenerator = parameterGenerator;
        _fileService = fileService;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            var command = Console.ReadLine()?.Trim();
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
                InvalidDataException or
                IOException or
                InvalidOperationException or
                ArithmeticException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void SignFile()
    {
        var algorithm = ReadAlgorithm();
        var keyPair = ReadKeyPair(algorithm);
        PrintKeyPair(keyPair);

        var inputPath = ReadRequiredText("Файл для подписания: ");
        var defaultSignaturePath = inputPath + ".sig.json";
        var signaturePath = ReadOptionalText(
            $"Файл подписи [{defaultSignaturePath}]: ",
            defaultSignaturePath);

        var result = _fileService.SignFile(inputPath, signaturePath, keyPair);
        Console.WriteLine($"SHA-256: {result.HashHex}");
        Console.WriteLine($"Размер хеша: {result.HashLength} байт");
        Console.WriteLine($"Подпись сохранена: {result.SignaturePath}");
    }

    private void VerifyFile()
    {
        var inputPath = ReadRequiredText("Файл для проверки: ");
        var defaultSignaturePath = inputPath + ".sig.json";
        var signaturePath = ReadOptionalText(
            $"Файл подписи [{defaultSignaturePath}]: ",
            defaultSignaturePath);

        var result = _fileService.VerifyFile(inputPath, signaturePath);
        Console.WriteLine(result.Message);
    }

    private void GenerateAndPrintParameters()
    {
        var algorithm = ReadAlgorithm();
        var keyPair = GenerateKeyPair(algorithm);
        PrintKeyPair(keyPair);
    }

    private SignatureKeyPair ReadKeyPair(string algorithm)
    {
        var generatorName = algorithm == Gost94SignatureAlgorithm.AlgorithmName ? "a" : "g";
        Console.WriteLine($"1 - ввод P, Q, {generatorName}, x");
        Console.WriteLine($"2 - генерация P, Q, {generatorName}, x, y");
        Console.Write("Режим: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => _parameterGenerator.Create(
                algorithm,
                ReadBigInteger("P: "),
                ReadBigInteger("Q: "),
                ReadBigInteger($"{generatorName}: "),
                ReadBigInteger("x (секретный ключ): ")),
            "2" => GenerateKeyPair(algorithm),
            _ => throw new ArgumentException("Неизвестный режим.")
        };
    }

    private SignatureKeyPair GenerateKeyPair(string algorithm)
    {
        var isGost = algorithm == Gost94SignatureAlgorithm.AlgorithmName;
        var defaultPBitLength = isGost ? 31 : 1024;
        var pBitLength = ReadInt("Разрядность P", defaultPBitLength);
        return _parameterGenerator.Generate(algorithm, pBitLength);
    }

    private static string ReadAlgorithm()
    {
        Console.WriteLine("1 - ГОСТ Р 34.10-94");
        Console.WriteLine("2 - DSA (FIPS 186)");
        Console.Write("Алгоритм: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => Gost94SignatureAlgorithm.AlgorithmName,
            "2" => DsaSignatureAlgorithm.AlgorithmName,
            _ => throw new ArgumentException("Неизвестный алгоритм.")
        };
    }

    private static void PrintKeyPair(SignatureKeyPair keyPair)
    {
        Console.WriteLine($"P: {keyPair.P}");
        Console.WriteLine($"Q: {keyPair.Q}");
        Console.WriteLine($"{keyPair.GeneratorParameterName}: {keyPair.Generator}");
        Console.WriteLine($"x (секретный ключ): {keyPair.X}");
        Console.WriteLine($"y (открытый ключ): {keyPair.Y}");
    }

    private static string ReadRequiredText(string prompt)
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
        if (!BigInteger.TryParse(Console.ReadLine(), out var value))
        {
            throw new FormatException("Ожидалось целое число.");
        }

        return value;
    }

    private static int ReadInt(string prompt, int defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
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
        Console.WriteLine("3 - Сгенерировать параметры подписи");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
