using System.IO;
using System.Security.Cryptography;

namespace EELanLauncher.Utils;

public static class HashUtil
{
    public static string Sha256OfFile(string path)
    {
        using var s = File.OpenRead(path);
        using var h = SHA256.Create();
        var b = h.ComputeHash(s);
        return string.Concat(System.BitConverter.ToString(b)).Replace("-", "");
    }
}
