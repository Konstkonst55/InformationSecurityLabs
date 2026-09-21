using System.Numerics;

namespace _4;

public interface IShamirTransformer
{
    BigInteger Apply(BigInteger value, BigInteger exponent, BigInteger p);
}
