using System.Numerics;

namespace _7;

public sealed record DiffieHellmanKeyResult(
    BigInteger P,
    BigInteger G,
    BigInteger Xa,
    BigInteger Xb,
    BigInteger Ya,
    BigInteger Yb,
    BigInteger Ka,
    BigInteger Kb);
