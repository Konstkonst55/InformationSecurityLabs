using _1;

namespace _2;

public sealed class DiscreteLogVerificationService : IVerificationService
{
    private readonly IModularExponentiation _modularExponentiation;

    public DiscreteLogVerificationService(IModularExponentiation modularExponentiation)
    {
        _modularExponentiation = modularExponentiation;
    }

    public string Verify(DiscreteLogResult result)
    {
        if (!result.HasSolution || result.X is null)
        {
            return "решение отсутствует";
        }

        var actual = _modularExponentiation.Calculate(result.A, result.X.Value, result.P);
        return actual == result.Y ? $"верно ({actual})" : $"ошибка ({actual} != {result.Y})";
    }
}
