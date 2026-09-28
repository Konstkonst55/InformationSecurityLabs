using System.Numerics;

namespace _5;

public sealed record ElGamalKeyPair(
    ElGamalPublicParameters PublicParameters,
    ElGamalPrivateParameters PrivateParameters);
