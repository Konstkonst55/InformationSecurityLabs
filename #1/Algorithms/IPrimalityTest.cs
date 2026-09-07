using System.Numerics;

namespace _1;

public interface IPrimalityTest
{
    bool IsProbablyPrime(BigInteger number, int iterations = 100);
}
