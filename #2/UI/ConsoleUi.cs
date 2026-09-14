using System.Numerics;

namespace _2;

public sealed class ConsoleUi
{
    private readonly IDiscreteLogarithmSolver _solver;
    private readonly DiscreteLogScenarioService _scenarioService;
    private readonly IVerificationService _verificationService;

    public ConsoleUi(
        IDiscreteLogarithmSolver solver,
        DiscreteLogScenarioService scenarioService,
        IVerificationService verificationService)
    {
        _solver = solver;
        _scenarioService = scenarioService;
        _verificationService = verificationService;
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
                        RunManual();
                        break;
                    case "2":
                        RunGenerated();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException or FormatException or OverflowException or InvalidOperationException)
            {
                Console.WriteLine(exception.Message);
            }

            Console.WriteLine();
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("1 - Ввод a, y, p");
        Console.WriteLine("2 - Сгенерировать параметры");
        Console.WriteLine("0 - Выход");
        Console.Write("Ввод: ");
    }

    private void RunManual()
    {
        var a = ReadBigInteger("a: ");
        var y = ReadBigInteger("y: ");
        var p = ReadBigInteger("p: ");
        var result = _solver.Solve(a, y, p);
        PrintResult(result);
    }

    private void RunGenerated()
    {
        var bits = ReadInt("Разрядность p: ", 20);
        var result = _scenarioService.SolveGenerated(bits);
        PrintResult(result);
        Console.WriteLine($"Проверка: {_verificationService.Verify(result)}");
    }

    private static void PrintResult(DiscreteLogResult result)
    {
        Console.WriteLine($"m: {result.M}");
        if (result.HasSolution)
        {
            Console.WriteLine($"x: {result.X}");
            Console.WriteLine($"a^x mod p = y, x = {result.X}");
        }
        else
        {
            Console.WriteLine("Решение в пределах 0 <= x < p - 1 не найдено.");
        }
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
