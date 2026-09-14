using System.Numerics;
using _2;

namespace _3;

public sealed class KeyAgreementVerificationService : IKeyAgreementVerificationService
{
    private readonly IDiscreteLogarithmSolver _discreteLogarithmSolver;

    public KeyAgreementVerificationService(IDiscreteLogarithmSolver discreteLogarithmSolver)
    {
        _discreteLogarithmSolver = discreteLogarithmSolver;
    }

    public bool Verify(DiffieHellmanResult result)
    {
        return result.AliceSharedKey == result.BobSharedKey;
    }

    public BigInteger RecoverPrivateKey(BigInteger g, BigInteger publicKey, BigInteger p)
    {
        var result = _discreteLogarithmSolver.Solve(g, publicKey, p);
        if (!result.HasSolution)
        {
            throw new InvalidOperationException("Секретный показатель не найден алгоритмом BSGS.");
        }

        return result.X!.Value;
    }
}
