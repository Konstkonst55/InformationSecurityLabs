using System.Numerics;

namespace _5;

public sealed record ElGamalPrivateParameters(
    BigInteger P,
    BigInteger Db);
