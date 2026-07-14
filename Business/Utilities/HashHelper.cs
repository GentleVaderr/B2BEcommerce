using System.Security.Cryptography;
using System.Text;

namespace Business.Utilities
{
    public static class HashHelper
    {
        public static string CreateMD5(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                // Gelen şifreyi byte dizisine çevirme
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // MD5 algoritması ile şifreleme
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Byte dizisini tekrar string dönüştürme
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2")); // X2: Büyük harfli Hex formatı
                }

                return sb.ToString();
            }
        }
    }
}