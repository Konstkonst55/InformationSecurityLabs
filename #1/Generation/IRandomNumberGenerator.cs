using System.Numerics;

namespace _1;

public interface IRandomNumberGenerator
{
    BigInteger Generate(BigInteger minInclusive, BigInteger maxInclusive);
    BigInteger GeneratePositive(int bitLength);
    BigInteger GeneratePrime(int bitLength, IPrimalityTest primalityTest, int iterations = 100);
}
