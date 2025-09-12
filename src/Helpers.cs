using System;
using System.IO;

namespace gmkeylib
{
    public static class Helpers
    {
        // On Mono/Linux, WMI isn't available. Use fallback UUID or env var.
        public static string GetUID()
        {
            // TODO: detect platform and plug in real machine ID if needed
            // For now hardcode your known UUID:
            return "4E594784-1E0B-FCB9-9589-4CEDFB3FBC54";
        }

        public static void WriteBinToFile(string FileName, byte[] Buf)
        {
            using (FileStream output = new FileStream(FileName, FileMode.Create))
            using (BinaryWriter binaryWriter = new BinaryWriter(output))
            {
                binaryWriter.Write(Buf);
            }
        }
    }
}
