using System.Numerics;

namespace _3;

public interface IDiffieHellmanAlgorithm
{
    DiffieHellmanResult Calculate(
        BigInteger p,
        BigInteger g,
        BigInteger alicePrivateKey,
        BigInteger bobPrivateKey);
}
