using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using InTheHand.Net.Sockets;


namespace HostLocker {
    //TODO: IMPLEMENT THIS, THE PURPOSE OF THIS CLASS IS TO SAVE SECURELY THE KEYS
    class DataProtector {

        public static void SaveData(UserData userData)
        {
            FileStream fStream = new FileStream($"C:\\Users\\extre\\Desktop\\user_test_data.dat", FileMode.OpenOrCreate);
            byte[] binaryObj = ObjectToByteArray(userData);
            EncryptDataToStream(binaryObj, DataProtectionScope.LocalMachine, fStream);

            fStream.Close();
        }

        public static UserData LoadData(string sap) {
            FileStream fStream = new FileStream($"C:\\Users\\extre\\Desktop\\user_{sap}_data.dat", FileMode.Open);
            byte[] decryptData = DecryptDataFromStream(DataProtectionScope.LocalMachine, fStream);
            fStream.Close();

            return (UserData)ByteArrayToObject(decryptData);
        }

        private static byte[] ObjectToByteArray(object obj) {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static int EncryptDataToStream(byte[] Buffer, DataProtectionScope Scope, Stream S) {
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

        private static byte[] DecryptDataFromStream(DataProtectionScope Scope, Stream S) {
            if (S == null)
                throw new ArgumentNullException("S");

            byte[] outBuffer;
            // Read the encrypted data from a stream.
            using (S)
            {
                if (S.CanRead) {
                    var inBuffer = new byte[S.Length];
                    S.Read(inBuffer, 0, (int)S.Length);
                    outBuffer = ProtectedData.Unprotect(inBuffer, null, Scope);
                }
                else {
                    throw new IOException("Could not read the stream.");
                }
            }

            // Return the byte array that contains the decrypted data. 
            return outBuffer;
        }

        private static Object ByteArrayToObject(byte[] arrBytes) {
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                Object obj = (Object)binForm.Deserialize(memStream);

                return obj;
            }
        }
    }

    [Serializable()]
    public class UserData
    {
        public byte[] UserSecretKey { get; set; }
        public string UserNonce { get; set; }
        public string EncryptedUserAesKey { get; set; }
        public byte[] UserAesInitVect { get; set; }
        //public BluetoothDeviceInfo BlDeviceInfo { get; set; }
        public RSAParameters UserPrivKey { get; set; }
        public RSAParameters UserPubKey { get; set; }
        public RSAParameters DevicePublicKey { get; set; }
        public List<string> files { get; set; }
    }
}
