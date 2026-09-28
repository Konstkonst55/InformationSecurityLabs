using System.Numerics;

namespace _5;

public sealed record ElGamalPublicParameters(
    BigInteger P,
    BigInteger G,
    BigInteger Cb);
