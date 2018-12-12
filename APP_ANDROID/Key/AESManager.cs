using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;


namespace Key
{
    public class AesManager
    {
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        private static int _blockSize = 128;
        private static int _keySize = 256;
        private static CipherMode _mode = CipherMode.CBC;
        private static PaddingMode _padding = PaddingMode.PKCS7;

        public byte[] Key { get; set; }
        public byte[] InitVect { get; set; }

        private AesCryptoServiceProvider SetupProvider()
        {
            AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider();
            aesAlg.BlockSize = _blockSize;
            aesAlg.KeySize = _keySize;
            aesAlg.Mode = _mode;
            aesAlg.Padding = _padding;

            return aesAlg;
        }

        public void InitKey()
        {
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider()) {
                aesAlg.GenerateIV();
                aesAlg.GenerateKey();

                Key = aesAlg.Key;
                InitVect = aesAlg.IV;
            }
        }

        public void SetKey(byte[] key, byte[] iv)
        {
            Key = key;
            InitVect = iv;
        }

        public byte[] StringToBytes(string content)
        {
            return Encoding.ASCII.GetBytes(content);
        }

        public byte[] EncryptStringToBytes_Aes(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (InitVect == null || InitVect.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and InitVect.
            using (AesCryptoServiceProvider aesAlg = SetupProvider())
            {
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(Key, InitVect);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (InitVect == null || InitVect.Length <= 0)
                throw new ArgumentNullException("IV");
            string plaintext;
            // Create an AesCryptoServiceProvider object
            // with the specified key and InitVect.
            using (AesCryptoServiceProvider aesAlg = SetupProvider())
            {
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(Key, InitVect);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}