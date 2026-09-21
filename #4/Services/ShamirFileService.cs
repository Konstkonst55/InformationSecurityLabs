using System.Numerics;

namespace _4;

public sealed class ShamirFileService : IShamirFileService
{
    private readonly IShamirTransformer _transformer;

    public ShamirFileService(IShamirTransformer transformer)
    {
        _transformer = transformer;
    }

    public EncryptionResult Encrypt(
        string inputPath,
        string outputPath,
        ShamirKeys keys)
    {
        ValidatePaths(inputPath, outputPath);
        ValidateKey(keys);

        var width = GetBlockWidth(keys.P);
        long count = 0;

        using var input = new FileStream(
            inputPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var output = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var buffer = new byte[8192];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (var i = 0; i < read; i++)
            {
                var m = new BigInteger(buffer[i]);
                var c1 = _transformer.Apply(m, keys.Ca, keys.P);
                var c2 = _transformer.Apply(c1, keys.Cb, keys.P);
                WriteFixed(c2, output, width);
                count++;
            }
        }

        return new EncryptionResult(
            inputPath,
            outputPath,
            keys.P,
            keys.Ca,
            keys.Da,
            keys.Cb,
            keys.Db,
            count);
    }

    public EncryptionResult Decrypt(
        string inputPath,
        string outputPath,
        ShamirKeys keys)
    {
        ValidatePaths(inputPath, outputPath);
        ValidateKey(keys);

        var width = GetBlockWidth(keys.P);
        long count = 0;

        using var input = new FileStream(
            inputPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var output = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        var block = new byte[width];
        while (true)
        {
            var read = ReadExact(input, block);
            if (read == 0)
            {
                break;
            }

            if (read != width)
            {
                throw new InvalidDataException("Размер зашифрованного файла не кратен размеру блока.");
            }

            var c2 = new BigInteger(
                block,
                isUnsigned: true,
                isBigEndian: true);

            if (c2 >= keys.P)
            {
                throw new InvalidDataException("Значение блока не соответствует модулю p.");
            }

            var c3 = _transformer.Apply(c2, keys.Da, keys.P);
            var m = _transformer.Apply(c3, keys.Db, keys.P);

            if (m > 255)
            {
                throw new InvalidDataException("Расшифрованный блок не является байтом.");
            }

            output.WriteByte((byte)m);
            count++;
        }

        return new EncryptionResult(
            inputPath,
            outputPath,
            keys.P,
            keys.Ca,
            keys.Da,
            keys.Cb,
            keys.Db,
            count);
    }

    private static int GetBlockWidth(BigInteger p)
    {
        return p.ToByteArray(isUnsigned: true, isBigEndian: true).Length;
    }

    private static void WriteFixed(
        BigInteger value,
        Stream output,
        int width)
    {
        var bytes = value.ToByteArray(
            isUnsigned: true,
            isBigEndian: true);

        if (bytes.Length > width)
        {
            throw new InvalidDataException("Зашифрованный блок не помещается в фиксированный блок.");
        }

        if (bytes.Length < width)
        {
            var padded = new byte[width];
            Buffer.BlockCopy(
                bytes,
                0,
                padded,
                width - bytes.Length,
                bytes.Length);
            bytes = padded;
        }

        output.Write(bytes, 0, bytes.Length);
    }

    private static int ReadExact(Stream input, byte[] buffer)
    {
        var total = 0;

        while (total < buffer.Length)
        {
            var read = input.Read(
                buffer,
                total,
                buffer.Length - total);

            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }

    private static void ValidatePaths(
        string inputPath,
        string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Исходный файл не найден.", inputPath);
        }

        if (string.Equals(
                Path.GetFullPath(inputPath),
                Path.GetFullPath(outputPath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Исходный и выходной файлы должны различаться.");
        }

        var parent = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }

    private static void ValidateKey(ShamirKeys keys)
    {
        if (keys.P <= 257)
        {
            throw new ArgumentException("p должно быть больше 257.");
        }
    }
}
