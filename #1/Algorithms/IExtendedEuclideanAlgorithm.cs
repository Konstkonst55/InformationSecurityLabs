using System.Numerics;

namespace _1;

public interface IExtendedEuclideanAlgorithm
{
    ExtendedGcdResult Calculate(BigInteger a, BigInteger b);
}
