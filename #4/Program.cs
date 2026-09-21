using _1;

namespace _4;

public static class Program
{
    public static void Main()
    {
        IModularExponentiation pow = new ModularExponentiation();
        IExtendedEuclideanAlgorithm gcd = new ExtendedEuclideanAlgorithm();
        IRandomNumberGenerator random = new SecureRandomNumberGenerator();
        IPrimalityTest primality = new FermatPrimalityTest(gcd, pow, random);
        IShamirKeyGenerator keyGenerator = new ShamirKeyGenerator(random, primality, gcd);
        IShamirTransformer transformer = new ShamirTransformer(pow);
        IShamirFileService fileService = new ShamirFileService(transformer);
        IParameterPresentationService presentation = new ParameterPresentationService();

        new ConsoleUi(keyGenerator, fileService, presentation).Run();
    }
}
