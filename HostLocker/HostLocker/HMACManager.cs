using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HostLocker {
    class HMACManager
    {

        public byte[] SecretKey {get; set;} = new Byte[64];

        public HMACManager()
        {
            RefreshSecretKey();
        }

        public string GetSecretKeyString()
        {
            return Encoding.ASCII.GetString(SecretKey);
        }

        public void RefreshSecretKey()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                // The array is now filled with cryptographically strong random bytes.
                rng.GetBytes(SecretKey);
            }
        }

        public string Encode(string message) {
            byte[] messageBytes = Encoding.Unicode.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(SecretKey)) {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);

                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
