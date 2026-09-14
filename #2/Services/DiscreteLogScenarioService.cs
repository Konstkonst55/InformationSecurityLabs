using System.Numerics;
using _1;

namespace _2;

public sealed class DiscreteLogScenarioService
{
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly IPrimalityTest _primalityTest;
    private readonly IModularExponentiation _modularExponentiation;
    private readonly IDiscreteLogarithmSolver _solver;

    public DiscreteLogScenarioService(
        IRandomNumberGenerator randomNumberGenerator,
        IPrimalityTest primalityTest,
        IModularExponentiation modularExponentiation,
        IDiscreteLogarithmSolver solver)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _primalityTest = primalityTest;
        _modularExponentiation = modularExponentiation;
        _solver = solver;
    }

    public DiscreteLogResult SolveGenerated(int bitLength, int primalityIterations = 40)
    {
        if (bitLength < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(bitLength), "Разрядность p должна быть не меньше 4.");
        }

        var p = _randomNumberGenerator.GeneratePrime(bitLength, _primalityTest, primalityIterations);
        var a = _randomNumberGenerator.Generate(2, p - 2);
        var x = _randomNumberGenerator.Generate(0, p - 2);
        var y = _modularExponentiation.Calculate(a, x, p);

        return _solver.Solve(a, y, p);
    }
}
