using System;
using System.IO;
using gmkeylib;   // reference your gmkeylib.dll

class TestHarness
{
    static void Main()
    {
        if (!File.Exists("gmkeylib.lic"))
        {
            Console.WriteLine("gmkeylib.lic missing!");
            return;
        }

        // Step 1: Read license file
        string[] lines = File.ReadAllLines("gmkeylib.lic");
        if (lines.Length < 2)
        {
            Console.WriteLine("Invalid .lic format");
            return;
        }

        string firstLine = lines[0];
        string encryptedLine = lines[1];

        Console.WriteLine($"Marker length = {firstLine.Length}");
        Console.WriteLine($"Encrypted payload (first 40 chars) = {encryptedLine.Substring(0, Math.Min(40, encryptedLine.Length))}...");

        // Step 2: Get UID like the real loader would
        string uid = Helpers.GetUID();
        string sharedSecret = uid + "/xmH5hRMA9QAOIk+fWrmFQ";

        // Step 3: Decrypt the payload into text3
        string text3;
        try
        {
            text3 = Crypto.DecryptStringAES(encryptedLine, sharedSecret);
            Console.WriteLine("Decrypted license payload (text3) = " + text3);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Decrypt failed: " + ex.Message);
            return;
        }

        // Step 4: Use gmkey.GetKey() with Algo 93 and a dummy seed
        Console.WriteLine("\nInvoking gmkey.GetKey with Algo 93...");
        byte[] seed = new byte[5] { 1, 2, 3, 4, 5 };  // dummy 5-byte seed
        try
        {
            byte[] key = gmkey.GetKey(seed, 93);
            if (key == null)
            {
                Console.WriteLine("gmkey.GetKey returned null");
            }
            else
            {
                // Print key as hex string for direct use
                string hexKey = BitConverter.ToString(key).Replace("-", "");
                Console.WriteLine("Final 5-byte key (hex) = " + hexKey);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("gmkey.GetKey threw exception: " + ex.Message);
        }
    }
}
