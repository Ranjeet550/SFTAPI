using System.Security.Cryptography;
using System.Text;

public static class Sha256Hasher
{
    public static string ComputeSHA256Hash(string rawData)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Convert input string to byte array
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
