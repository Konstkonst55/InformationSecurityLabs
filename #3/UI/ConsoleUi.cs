using System.Numerics;

namespace _3;

public sealed class ConsoleUi
{
    private readonly IDiffieHellmanScenarioService _scenarioService;
    private readonly IKeyAgreementVerificationService _verificationService;

    public ConsoleUi(
        IDiffieHellmanScenarioService scenarioService,
        IKeyAgreementVerificationService verificationService)
    {
        _scenarioService = scenarioService;
        _verificationService = verificationService;
    }

    public void Run()
    {
        DiffieHellmanResult? currentResult = null;

        while (true)
        {
            PrintMenu(currentResult is not null);
            var command = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (command)
                {
                    case "1":
                        currentResult = RunManualScenario();
                        PrintResult(currentResult);
                        break;
                    case "2":
                        currentResult = RunGeneratedScenario();
                        PrintResult(currentResult);
                        break;
                    case "3":
                        EnsureResult(currentResult);
                        Console.WriteLine(_verificationService.Verify(currentResult)
                            ? "Общие ключи совпадают."
                            : "Общие ключи не совпадают.");
                        break;
                    case "4":
                        EnsureResult(currentResult);
                        RecoverPrivateKey(currentResult);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException or InvalidOperationException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private DiffieHellmanResult RunManualScenario()
    {
        var p = ReadBigInteger("p: ");
        var g = ReadBigInteger("g: ");
        var alicePrivateKey = ReadBigInteger("Xa: ");
        var bobPrivateKey = ReadBigInteger("Xb: ");
        return _scenarioService.CalculateScenario(p, g, alicePrivateKey, bobPrivateKey);
    }

    private DiffieHellmanResult RunGeneratedScenario()
    {
        var bitLength = ReadInt("Разрядность p: ", 32);
        return _scenarioService.GenerateScenario(bitLength);
    }

    private void RecoverPrivateKey(DiffieHellmanResult result)
    {
        if (result.P > 10_000_000)
        {
            throw new InvalidOperationException("Проверка через BSGS предназначена для небольших p и отключена для p > 10000000.");
        }

        var aliceRecovered = _verificationService.RecoverPrivateKey(
            result.G,
            result.AlicePublicKey,
            result.P);

        var bobRecovered = _verificationService.RecoverPrivateKey(
            result.G,
            result.BobPublicKey,
            result.P);

        Console.WriteLine($"Восстановлен Xa: {aliceRecovered}");
        Console.WriteLine($"Восстановлен Xb: {bobRecovered}");
        Console.WriteLine($"Фактический Xa: {result.AlicePrivateKey}");
        Console.WriteLine($"Фактический Xb: {result.BobPrivateKey}");
    }

    private static void PrintResult(DiffieHellmanResult result)
    {
        Console.WriteLine($"p: {result.P}");
        Console.WriteLine($"g: {result.G}");
        Console.WriteLine($"Xa: {result.AlicePrivateKey}");
        Console.WriteLine($"Xb: {result.BobPrivateKey}");
        Console.WriteLine($"Ya: {result.AlicePublicKey}");
        Console.WriteLine($"Yb: {result.BobPublicKey}");
        Console.WriteLine($"Ka: {result.AliceSharedKey}");
        Console.WriteLine($"Kb: {result.BobSharedKey}");
        Console.WriteLine($"Проверка Ka == Kb: {result.AliceSharedKey == result.BobSharedKey}");
    }

    private static void EnsureResult(DiffieHellmanResult? result)
    {
        if (result is null)
        {
            throw new InvalidOperationException("Сначала выполните расчёт параметров.");
        }
    }

    private static void PrintMenu(bool hasResult)
    {
        Console.WriteLine("1 - Ввод p, g, Xa, Xb");
        Console.WriteLine("2 - Генерация параметров и секретных ключей");
        Console.WriteLine("3 - Проверка совпадения общего ключа");
        Console.WriteLine("4 - Восстановить секреты через BSGS");
        Console.WriteLine("0 - Выход");
        if (!hasResult)
        {
            Console.WriteLine("Расчёт ещё не выполнен.");
        }
        Console.Write("Ввод: ");
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
