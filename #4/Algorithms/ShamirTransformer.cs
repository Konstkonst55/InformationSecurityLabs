using System.Numerics;
using _1;

namespace _4;

public sealed class ShamirTransformer : IShamirTransformer
{
    private readonly IModularExponentiation _pow;

    public ShamirTransformer(IModularExponentiation pow)
    {
        _pow = pow;
    }

    public BigInteger Apply(BigInteger value, BigInteger exponent, BigInteger p)
    {
        return _pow.Calculate(value, exponent, p);
    }
}
