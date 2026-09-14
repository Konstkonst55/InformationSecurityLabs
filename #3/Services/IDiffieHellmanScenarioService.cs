using System.Numerics;

namespace _3;

public interface IDiffieHellmanScenarioService
{
    DiffieHellmanResult GenerateScenario(int bitLength);
    DiffieHellmanResult CalculateScenario(BigInteger p, BigInteger g, BigInteger alicePrivateKey, BigInteger bobPrivateKey);
}
