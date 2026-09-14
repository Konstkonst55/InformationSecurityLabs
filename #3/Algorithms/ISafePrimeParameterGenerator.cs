namespace _3;

public interface ISafePrimeParameterGenerator
{
    DiffieHellmanParameters Generate(int bitLength, int primalityIterations = 20);
}
