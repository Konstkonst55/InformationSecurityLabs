using System.Numerics;

namespace _2;

public sealed record DiscreteLogResult(
    BigInteger A,
    BigInteger Y,
    BigInteger P,
    BigInteger M,
    BigInteger? X,
    bool HasSolution);
