using System.Numerics;

namespace _2;

public interface IDiscreteLogarithmSolver
{
    DiscreteLogResult Solve(BigInteger a, BigInteger y, BigInteger p);
}
