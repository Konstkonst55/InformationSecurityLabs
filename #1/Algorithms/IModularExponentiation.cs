using System.Numerics;

namespace _1;

public interface IModularExponentiation
{
    BigInteger Calculate(BigInteger a, BigInteger exponent, BigInteger modulus);
}
