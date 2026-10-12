using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace _7;

public sealed class VernamKeyFileService
{
    private const string DiffieHellmanHeader = "DH-K1";

    public void GenerateOneTimeKey(string inputPath, string keyPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException(
                "Исходный файл не найден.",
                inputPath);
        }

        EnsureDifferentPaths(inputPath, keyPath);
        EnsureParentDirectory(keyPath);

        var remaining = new FileInfo(inputPath).Length;
        using var output = new FileStream(
            keyPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var buffer = new byte[8192];
        while (remaining > 0)
        {
            var length = (int)Math.Min(buffer.Length, remaining);
            RandomNumberGenerator.Fill(buffer.AsSpan(0, length));
            output.Write(buffer, 0, length);
            remaining -= length;
        }
    }

    public void SaveDiffieHellmanKey(BigInteger key, string keyPath)
    {
        if (key <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(key),
                "Ключ K должен быть положительным.");
        }

        EnsureParentDirectory(keyPath);
        var contents = string.Concat(
            DiffieHellmanHeader,
            Environment.NewLine,
            key.ToString(CultureInfo.InvariantCulture),
            Environment.NewLine);
        File.WriteAllText(keyPath, contents, new UTF8Encoding(false));
    }

    public BigInteger LoadDiffieHellmanKey(string keyPath)
    {
        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException(
                "Файл ключа Диффи—Хеллмана не найден.",
                keyPath);
        }

        var lines = File.ReadAllLines(keyPath, Encoding.UTF8);
        if (lines.Length < 2 || lines[0] != DiffieHellmanHeader)
        {
            throw new InvalidDataException(
                "Файл не содержит ключ K, полученный методом Диффи—Хеллмана.");
        }

        if (!BigInteger.TryParse(
                lines[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var key) || key <= 0)
        {
            throw new InvalidDataException(
                "Файл содержит некорректное значение K.");
        }

        return key;
    }

    public void ValidateOneTimeKeyLength(
        string inputPath,
        string keyPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException(
                "Исходный файл не найден.",
                inputPath);
        }

        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException(
                "Файл одноразового ключа не найден.",
                keyPath);
        }

        var inputLength = new FileInfo(inputPath).Length;
        var keyLength = new FileInfo(keyPath).Length;

        if (inputLength != keyLength)
        {
            throw new InvalidDataException(
                "Длина файла одноразового ключа должна совпадать с длиной исходного файла.");
        }
    }

    public static void EnsureDifferentPaths(params string[] paths)
    {
        var fullPaths = paths
            .Select(Path.GetFullPath)
            .ToArray();

        for (var i = 0; i < fullPaths.Length; i++)
        {
            for (var j = i + 1; j < fullPaths.Length; j++)
            {
                if (string.Equals(
                        fullPaths[i],
                        fullPaths[j],
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Пути к исходному файлу, ключу и результату должны различаться.");
                }
            }
        }
    }

    private static void EnsureParentDirectory(string path)
    {
        var parent = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }
}
