using System.Numerics;

namespace _6;

public sealed record RsaParameters(
    BigInteger P,
    BigInteger Q,
    BigInteger N,
    BigInteger Phi,
    BigInteger D,
    BigInteger C);
