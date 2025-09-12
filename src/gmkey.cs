using System;
using System.IO;
using System.Windows.Forms;

namespace gmkeylib
{
    public static class gmkey
    {
        public static byte[] GetKey(byte[] SeedBytes, byte Algo)
        {
            if (!File.Exists("gmkeylib.lic"))
            {
                MessageBox.Show("License file missing", "GM Keys licensing");
                return null;
            }

            try
            {
                return LibLoader.GetKey(SeedBytes, Algo);
            }
            catch (Exception)
            {
                return new byte[1];
            }
        }
    }
}
