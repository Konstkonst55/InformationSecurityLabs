using System.Numerics;

namespace _4;

public interface IShamirKeyGenerator
{
    ShamirKeys Generate(int bitLength = 32, int primalityIterations = 20);
    ShamirKeys FromParameters(BigInteger p, BigInteger ca, BigInteger cb);
}
