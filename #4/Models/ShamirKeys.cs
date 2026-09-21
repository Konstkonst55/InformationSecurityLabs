using System.Numerics;

namespace _4;

public sealed record ShamirKeys(
    BigInteger P,
    BigInteger Ca,
    BigInteger Da,
    BigInteger Cb,
    BigInteger Db);
