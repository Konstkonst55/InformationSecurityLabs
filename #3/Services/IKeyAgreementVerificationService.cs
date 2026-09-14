using System.Numerics;

namespace _3;

public interface IKeyAgreementVerificationService
{
    bool Verify(DiffieHellmanResult result);
    BigInteger RecoverPrivateKey(BigInteger g, BigInteger publicKey, BigInteger p);
}
