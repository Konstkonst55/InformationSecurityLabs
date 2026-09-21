using System.Numerics;

namespace _4;

public sealed class ConsoleUi
{
    private readonly IShamirKeyGenerator _keyGenerator;
    private readonly IShamirFileService _fileService;
    private readonly IParameterPresentationService _presentation;

    public ConsoleUi(
        IShamirKeyGenerator keyGenerator,
        IShamirFileService fileService,
        IParameterPresentationService presentation)
    {
        _keyGenerator = keyGenerator;
        _fileService = fileService;
        _presentation = presentation;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("1 - Шифрование файла");
            Console.WriteLine("2 - Расшифрование файла");
            Console.WriteLine("3 - Сгенерировать параметры Шамира");
            Console.WriteLine("0 - Выход");
            Console.Write("Ввод: ");
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
                        Generate();
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
                FileNotFoundException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void Encrypt()
    {
        var mode = ReadMode(
            "1 - ввод p, Ca, Cb\n" +
            "2 - генерация p, Ca, Cb, Da, Db\n" +
            "Режим: ");

        var keys = mode == "1"
            ? _keyGenerator.FromParameters(
                ReadBigInteger("p: "),
                ReadBigInteger("Ca: "),
                ReadBigInteger("Cb: "))
            : _keyGenerator.Generate(ReadInt("Разрядность p: ", 32));

        _presentation.Print(keys);

        var input = ReadText("Файл для шифрования: ");
        var output = ReadText("Файл с шифротекстом: ");
        var result = _fileService.Encrypt(input, output, keys);

        Console.WriteLine($"Зашифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Decrypt()
    {
        var p = ReadBigInteger("p: ");
        var da = ReadBigInteger("Da: ");
        var db = ReadBigInteger("Db: ");
        var keys = new ShamirKeys(p, BigInteger.Zero, da, BigInteger.Zero, db);

        var input = ReadText("Файл для расшифрования: ");
        var output = ReadText("Исходный файл после расшифрования: ");
        var result = _fileService.Decrypt(input, output, keys);

        Console.WriteLine($"Расшифровано байт: {result.ProcessedBytes}");
        Console.WriteLine($"Результат: {result.OutputPath}");
    }

    private void Generate()
    {
        var keys = _keyGenerator.Generate(ReadInt("Разрядность p: ", 32));
        _presentation.Print(keys);
    }

    private static string ReadMode(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();

        if (value != "1" && value != "2")
        {
            throw new ArgumentException("Неизвестный режим.");
        }

        return value;
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
}
