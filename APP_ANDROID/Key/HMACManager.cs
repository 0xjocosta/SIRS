using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Key {
    class HMACManager
    {

        public byte[] SecretKey {get; set;}

        public HMACManager(byte[] digestKey)
        {
            SecretKey = digestKey;
        }

        public string GetSecretKeyString()
        {
            return Encoding.ASCII.GetString(SecretKey);
        }

        public string Encode(string message) {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(SecretKey)) {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);

                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
