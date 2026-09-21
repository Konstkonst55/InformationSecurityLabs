namespace _4;

public sealed class ParameterPresentationService : IParameterPresentationService
{
    public void Print(ShamirKeys keys)
    {
        Console.WriteLine($"p: {keys.P}");
        Console.WriteLine($"Ca: {keys.Ca}");
        Console.WriteLine($"Da: {keys.Da}");
        Console.WriteLine($"Cb: {keys.Cb}");
        Console.WriteLine($"Db: {keys.Db}");
        Console.WriteLine($"Проверка Ca * Da mod (p - 1): {(keys.Ca * keys.Da) % (keys.P - 1)}");
        Console.WriteLine($"Проверка Cb * Db mod (p - 1): {(keys.Cb * keys.Db) % (keys.P - 1)}");
    }
}
