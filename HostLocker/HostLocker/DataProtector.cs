using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace HostLocker {
    //TODO: IMPLEMENT THIS, THE PURPOSE OF THIS CLASS IS TO SAVE SECURELY THE KEYS
    class DataProtector {
        public static void Run() {
            try {
                //TODO: MAYBE USE ENTROPY AND STORE IT IN THE FIRST BYTES OF THE GENERATED FILE 
                // Create the original data to be encrypted
                byte[] toEncrypt = UnicodeEncoding.ASCII.GetBytes("This is some data of any length.");

                // Create a file.
                FileStream fStream = new FileStream("Data.dat", FileMode.OpenOrCreate);

                Console.WriteLine();
                Console.WriteLine("Original data: " + UnicodeEncoding.ASCII.GetString(toEncrypt));
                Console.WriteLine("Encrypting and writing to disk...");

                // Encrypt a copy of the data to the stream.
                int bytesWritten = EncryptDataToStream(toEncrypt, DataProtectionScope.CurrentUser, fStream);

                fStream.Close();

                Console.WriteLine("Reading data from disk and decrypting...");

                // Open the file.
                fStream = new FileStream("Data.dat", FileMode.Open);

                // Read from the stream and decrypt the data.
                byte[] decryptData = DecryptDataFromStream(DataProtectionScope.CurrentUser, fStream, bytesWritten);

                fStream.Close();

                Console.WriteLine("Decrypted data: " + UnicodeEncoding.ASCII.GetString(decryptData));


            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }

        }

        public static int EncryptDataToStream(byte[] Buffer, DataProtectionScope Scope, Stream S) {
            if (Buffer == null)
                throw new ArgumentNullException("Buffer");
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");
            if (S == null)
                throw new ArgumentNullException("S");

            int length = 0;

            // Encrypt the data in memory. The result is stored in the same same array as the original data.
            byte[] encryptedData = ProtectedData.Protect(Buffer, null, Scope);

            // Write the encrypted data to a stream.
            if (S.CanWrite && encryptedData != null) {
                S.Write(encryptedData, 0, encryptedData.Length);

                length = encryptedData.Length;
            }

            // Return the length that was written to the stream. 
            return length;

        }

        public static byte[] DecryptDataFromStream(DataProtectionScope Scope, Stream S, int Length) {
            if (S == null)
                throw new ArgumentNullException("S");
            if (Length <= 0)
                throw new ArgumentException("Length");

            byte[] inBuffer = new byte[Length];
            byte[] outBuffer;

            // Read the encrypted data from a stream.
            if (S.CanRead) {
                S.Read(inBuffer, 0, Length);

                outBuffer = ProtectedData.Unprotect(inBuffer, null, Scope);
            }
            else {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream. 
            return outBuffer;

        }
    }
}
