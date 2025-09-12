using System;
using System.IO;
using System.Security.Cryptography;

namespace gmkeylib
{
    public static class Crypto
    {
        private static byte[] _salt = new byte[11]
        {
            63, 40, 242, 234, 68, 188, 45, 42, 69, 230,
            74
        };

        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            RijndaelManaged rijndaelManaged = null;
            try
            {
                var rfc2898 = new Rfc2898DeriveBytes("+qjy3+" + sharedSecret, _salt);
                rijndaelManaged = new RijndaelManaged();
                rijndaelManaged.Key = rfc2898.GetBytes(rijndaelManaged.KeySize / 8);

                ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(BitConverter.GetBytes(rijndaelManaged.IV.Length), 0, 4);
                    ms.Write(rijndaelManaged.IV, 0, rijndaelManaged.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            finally
            {
                if (rijndaelManaged != null) rijndaelManaged.Clear();
            }
        }

        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            RijndaelManaged rijndaelManaged = null;
            try
            {
                var rfc2898 = new Rfc2898DeriveBytes("+qjy3+" + sharedSecret, _salt);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    rijndaelManaged = new RijndaelManaged();
                    rijndaelManaged.Key = rfc2898.GetBytes(rijndaelManaged.KeySize / 8);
                    rijndaelManaged.IV = ReadByteArray(ms);

                    ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (rijndaelManaged != null) rijndaelManaged.Clear();
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[4];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
                throw new SystemException("Stream did not contain properly formatted byte array");

            int length = BitConverter.ToInt32(rawLength, 0);
            byte[] buffer = new byte[length];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new SystemException("Did not read byte array properly");

            return buffer;
        }
    }
}

