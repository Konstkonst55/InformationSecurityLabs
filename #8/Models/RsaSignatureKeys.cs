using System.Numerics;

namespace _8;

public sealed record RsaSignatureKeys(
    BigInteger P,
    BigInteger Q,
    BigInteger N,
    BigInteger Phi,
    BigInteger D,
    BigInteger C);
