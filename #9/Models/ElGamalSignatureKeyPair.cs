using System.Numerics;

namespace _9;

public sealed record ElGamalSignatureKeyPair(
    ElGamalSignaturePublicKey PublicKey,
    BigInteger X);
