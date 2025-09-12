using System;
using System.IO;
using System.Security.Cryptography;

class LicGen
{
    private static byte[] _salt = new byte[11] { 63, 40, 242, 234, 68, 188, 45, 42, 69, 230, 74 };

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: mono LicGen.exe <UID>");
            return;
        }

        string uid = args[0];
        string sharedSecret = uid + "/xmH5hRMA9QAOIk+fWrmFQ";

        // Step 1: inner = actual blob from passwords[93]
        string inner = "EAAAAHihJOGvb8wVUgDxAzH7mGTm7owaZIRXOItsWKHd8JkUlO3AvnFHwQUt5T094QXIYbCGolnuBL8bdvpq9TDb+xFIv9mXQfPDuxVyICosbnSH";
        Console.WriteLine("Stage1 (password[93] blob) = " + inner);

        // Step 2: AES encrypt that blob with UID secret (outer encryption)
        string outer = EncryptStringAES(inner, sharedSecret);
        Console.WriteLine("Outer encrypted = " + outer.Substring(0, Math.Min(outer.Length, 60)) + "...");

        // Step 3: Write gmkeylib.lic
        using (StreamWriter sw = new StreamWriter("gmkeylib.lic"))
        {
            sw.WriteLine("X");    // marker line (length = 1)
            sw.WriteLine(outer);  // encrypted blob
        }

        Console.WriteLine("gmkeylib.lic written for UID=" + uid);
    }

    private static string EncryptStringAES(string plainText, string sharedSecret)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));
        if (string.IsNullOrEmpty(sharedSecret))
            throw new ArgumentNullException(nameof(sharedSecret));

        RijndaelManaged rijndaelManaged = null;
        try
        {
            var key = new Rfc2898DeriveBytes("+qjy3+" + sharedSecret, _salt);
            rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = key.GetBytes(rijndaelManaged.KeySize / 8);

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
            rijndaelManaged?.Clear();
        }
    }
}
