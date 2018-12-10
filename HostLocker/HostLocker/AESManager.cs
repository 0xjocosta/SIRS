using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;


namespace HostLocker
{
    public class AesManager
    {
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        private static int _blockSize = 128;
        private static int _keySize = 256;
        private static CipherMode _mode = CipherMode.CFB;
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
        public void EncryptFile_Aes(string inputFile) {
            // Check arguments.
            if (inputFile == null || inputFile.Length <= 0)
                throw new ArgumentNullException(nameof(inputFile));

            // Create an AesCryptoServiceProvider object
            // with the specified key and InitVect.
            using (AesCryptoServiceProvider aesAlg = SetupAesProvider()) {
                aesAlg.GenerateIV();
                aesAlg.GenerateKey();

                Update(aesAlg.Key, aesAlg.IV);

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create)){
                    using (CryptoStream csEncrypt = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write)) {
                        using (FileStream fsIn = new FileStream(inputFile, FileMode.Open)) {
                            byte[] buffer = new byte[1048576];
                            int read;

                            try {
                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0) {
                                    //Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                                    csEncrypt.Write(buffer, 0, read);
                                }

                                // Close up
                                fsIn.Close();
                            }
                            catch (Exception ex) {
                                Console.WriteLine("Error: " + ex.Message);
                            }
                            finally {
                                fsIn.Close();
                                csEncrypt.Close();
                                fsCrypt.Close();
                            }
                        }
                    }
                }
            }
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

        public void DecryptFile_Aes(string inputFile, string outputFile) {
            // Check arguments.
            if (inputFile == null || inputFile.Length <= 0)
                throw new ArgumentNullException(nameof(inputFile));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (InitVect == null || InitVect.Length <= 0)
                throw new ArgumentNullException("IV");
            // Create an AesCryptoServiceProvider object
            // with the specified key and InitVect.

            using (AesCryptoServiceProvider aesAlg = SetupAesProvider()) {
                aesAlg.Key = Key;
                aesAlg.IV = InitVect;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open)) {
                    using (CryptoStream csDecrypt = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read)) {
                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create)) {
                            int read;
                            byte[] buffer = new byte[1048576];

                            try {
                                while ((read = csDecrypt.Read(buffer, 0, buffer.Length)) > 0) {
                                    //Application.DoEvents();
                                    fsOut.Write(buffer, 0, read);
                                }
                            }
                            catch (CryptographicException ex_CryptographicException) {
                                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                            }
                            catch (Exception ex) {
                                Console.WriteLine("Error: " + ex.Message);
                            }
                            finally {
                                fsOut.Close();
                                csDecrypt.Close();
                                fsCrypt.Close();
                            }
                        }
                    }
                }
            }
        }
    }
}