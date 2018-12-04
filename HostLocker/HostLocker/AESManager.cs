using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace HostLocker
{
    class AesManager
    {
        private static int _blockSize = 128;
        private static int _keySize = 256;
        private static CipherMode _mode = CipherMode.CBC;
        private static PaddingMode _padding = PaddingMode.PKCS7;

        public byte[] Key { get; set; }
        public byte[] InitVect { get; set; }

        public void Update(byte[] key, byte[] iv)
        {
            Key = key;
            InitVect = iv;
        }

        public byte[] StringToBytes(string content)
        {
            return Encoding.ASCII.GetBytes(content);
        }


        //FIX: for CBC mode, the InitVect must never be reused for different messages under the same key,
        //     and must be unpredictable in advance by an attacker
        public byte[] EncryptStringToBytes_Aes(string plainText) {
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
            using (AesCryptoServiceProvider aesAlg = SetupAesProvider()) {
                aesAlg.GenerateIV();
                aesAlg.GenerateKey();

                Update(aesAlg.Key, aesAlg.IV);

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private static AesCryptoServiceProvider SetupAesProvider()
        {
            AesCryptoServiceProvider aes_provider = new AesCryptoServiceProvider();
            aes_provider.BlockSize = _blockSize;
            aes_provider.KeySize = _keySize;
            aes_provider.Mode = _mode;
            aes_provider.Padding = _padding;

            return aes_provider;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText) {
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
            using (AesCryptoServiceProvider aesAlg = SetupAesProvider()) {
                aesAlg.Key = Key;
                aesAlg.IV = InitVect;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {

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