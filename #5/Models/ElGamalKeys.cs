using System.Numerics;

namespace _5;

public sealed record ElGamalKeys(
    BigInteger P,
    BigInteger G,
    BigInteger Cb,
    BigInteger Db);
