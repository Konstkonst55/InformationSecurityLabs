using System.Numerics;

namespace _9;

public sealed record ElGamalSignaturePublicKey(
    BigInteger P,
    BigInteger Q,
    BigInteger G,
    BigInteger Y);
