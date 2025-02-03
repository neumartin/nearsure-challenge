using System.IO.Compression;
using System.Text;

namespace LiveGameManager;

public class CompressionManager
{
    public static byte[] Compress(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        using var outputStream = new MemoryStream();
        using (var gzip = new GZipStream(outputStream, CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return outputStream.ToArray();
    }

    public static string Decompress(byte[] compressedData)
    {
        using var inputStream = new MemoryStream(compressedData);
        using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}