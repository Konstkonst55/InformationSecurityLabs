using System.Numerics;
using _1;
using Shared;

namespace _7;

public sealed class ConsoleUi
{
    private readonly VernamFileService _vernamFileService;
    private readonly VernamKeyFileService _keyFileService;
    private readonly DiffieHellmanKeyGenerator _diffieHellmanKeyGenerator;

    public ConsoleUi(
        VernamFileService vernamFileService,
        VernamKeyFileService keyFileService,
        DiffieHellmanKeyGenerator diffieHellmanKeyGenerator)
    {
        _vernamFileService = vernamFileService;
        _keyFileService = keyFileService;
        _diffieHellmanKeyGenerator = diffieHellmanKeyGenerator;
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
                        GenerateDiffieHellmanKey();
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
                InvalidOperationException or
                IOException or
                InvalidDataException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void Encrypt()
    {
        Console.WriteLine("1 - одноразовый ключевой файл той же длины, что и исходный файл");
        Console.WriteLine("2 - ключ K, полученный методом Диффи—Хеллмана");
        Console.Write("Режим: ");
        var mode = Console.ReadLine();

        var inputPath = ReadText("Исходный файл: ");
        var outputPath = ReadText("Файл шифротекста: ");
        var keyPath = ReadText("Файл ключа: ");

        FileProcessResult result;
        if (mode == "1")
        {
            VernamKeyFileService.EnsureDifferentPaths(
                inputPath,
                outputPath,
                keyPath);
            _keyFileService.GenerateOneTimeKey(inputPath, keyPath);
            result = _vernamFileService.EncryptWithOneTimeKey(
                inputPath,
                outputPath,
                keyPath);
            Console.WriteLine("Создан случайный одноразовый ключевой файл.");
        }
        else if (mode == "2")
        {
            result = _vernamFileService.EncryptWithDiffieHellmanKey(
                inputPath,
                outputPath,
                keyPath);
        }
        else
        {
            throw new ArgumentException("Неизвестный режим.");
        }

        Console.WriteLine($"Зашифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Decrypt()
    {
        Console.WriteLine("1 - одноразовый ключевой файл");
        Console.WriteLine("2 - ключ K, полученный методом Диффи—Хеллмана");
        Console.Write("Режим: ");
        var mode = Console.ReadLine();

        var inputPath = ReadText("Файл шифротекста: ");
        var outputPath = ReadText("Файл после расшифрования: ");
        var keyPath = ReadText("Файл ключа: ");

        FileProcessResult result = mode switch
        {
            "1" => _vernamFileService.DecryptWithOneTimeKey(
                inputPath,
                outputPath,
                keyPath),
            "2" => _vernamFileService.DecryptWithDiffieHellmanKey(
                inputPath,
                outputPath,
                keyPath),
            _ => throw new ArgumentException("Неизвестный режим.")
        };

        Console.WriteLine($"Расшифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void GenerateDiffieHellmanKey()
    {
        Console.WriteLine("1 - ввод p, g, Xa, Xb");
        Console.WriteLine("2 - генерация p, g, Xa, Xb");
        Console.Write("Режим: ");
        var mode = Console.ReadLine();

        DiffieHellmanKeyResult result;
        if (mode == "1")
        {
            result = _diffieHellmanKeyGenerator.Calculate(
                ReadBigInteger("p: "),
                ReadBigInteger("g: "),
                ReadBigInteger("Xa: "),
                ReadBigInteger("Xb: "));
        }
        else if (mode == "2")
        {
            result = _diffieHellmanKeyGenerator.Generate(
                ReadInt("Разрядность p: ", 32));
        }
        else
        {
            throw new ArgumentException("Неизвестный режим.");
        }

        PrintDiffieHellmanParameters(result);
        var keyPath = ReadText("Файл для сохранения K: ");
        _keyFileService.SaveDiffieHellmanKey(result.Ka, keyPath);
        Console.WriteLine($"Ключ K сохранён: {keyPath}");
    }

    private static void PrintDiffieHellmanParameters(
        DiffieHellmanKeyResult result)
    {
        Console.WriteLine($"p: {result.P}");
        Console.WriteLine($"g: {result.G}");
        Console.WriteLine($"Xa: {result.Xa}");
        Console.WriteLine($"Xb: {result.Xb}");
        Console.WriteLine($"Ya = g^Xa mod p: {result.Ya}");
        Console.WriteLine($"Yb = g^Xb mod p: {result.Yb}");
        Console.WriteLine($"Ka = Yb^Xa mod p: {result.Ka}");
        Console.WriteLine($"Kb = Ya^Xb mod p: {result.Kb}");
        Console.WriteLine($"Ka == Kb: {result.Ka == result.Kb}");
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
        Console.WriteLine("1 - Шифрование файла (шифр Вернама)");
        Console.WriteLine("2 - Расшифрование файла");
        Console.WriteLine("3 - Генерация ключа K методом Диффи—Хеллмана");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }
}
