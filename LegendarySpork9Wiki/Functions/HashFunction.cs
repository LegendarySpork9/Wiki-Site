using System.Security.Cryptography;
using System.Text;

namespace LegendarySpork9Wiki.Functions
{
    public static class HashFunction
    {
        public static string ComputeSHA512(string input)
        {
            byte[] bytes = SHA512.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
