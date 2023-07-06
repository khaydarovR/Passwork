using System.Text;

namespace Passwork.Server.Utils;

public static class KeyGenerator
{
    public static string GetRandomString(int lenth = 4)
    {
        var code = new char[lenth];
        Random r = new Random();
        var result = new StringBuilder();
        for (int i = 0; i < lenth; i++)
        {
            result.Append((char)r.NextInt64(97, 122));
        }

        return result.ToString();
    }
}
