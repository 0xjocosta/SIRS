using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Java.Security;
using Android.Security.Keystore;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace Key
{
    public class SecurityManager
    {
        private static string KEYSTORE_NAME = "AndroidKeyStore";
        private static string KEY_ALIAS = "PairKeys";
        private static int KEY_SIZE = 2048;
        private KeyStore keyStore;

        private RSAManager rsaManager;
        private string Nonce { get; set; }

        //variables to save
        private static string nonce = "Nonce";
        private static string KeycPub = "KcPub";
        private static string KeyDigest = "Kd";

        private static string KeyPub = "KsPub";
        private static string KeyPriv = "KsPri";
               
        public SecurityManager()
        {
            keyStore = KeyStore.GetInstance(KEYSTORE_NAME);          
            keyStore.Load(null);
            GenerateNonce();
            //GeneratePairKeys();
            rsaManager = new RSAManager();
            Console.WriteLine("PUBLIC" + rsaManager.PubKey);
            Console.WriteLine("PRIVATE" + rsaManager.PrivKey);
        }

        public string GenerateNonce()
        {
            Nonce = Guid.NewGuid().ToString("N");
            return Nonce;
        }

        private void test()
        {
            // Get the KeyGenerator instance for RSA
            KeyGenerator keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa);
            KeyPairGenerator keyPairGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa);

            // Create a KeyGenParameterSpec builder and 
            // set the alias and different purposes of the key
            KeyGenParameterSpec.Builder builder = new KeyGenParameterSpec.Builder(
                "myAwesomeSecretKey01", KeyStorePurpose.Decrypt | KeyStorePurpose.Encrypt);
            //KeyGen

            // The KeyGenParameterSpec is how parameters for your key are passed to the generator.
            builder.SetKeySize(2048);

            keyGenerator.Init(builder.Build());
            keyGenerator.GenerateKey();
        }

        private void GeneratePairKeys()
        {
            try
            {
                KeyGenParameterSpec builder = new KeyGenParameterSpec.Builder(KEY_ALIAS, 
                KeyStorePurpose.Decrypt | KeyStorePurpose.Encrypt)
                    .SetKeySize(2048)
                    .Build();

                KeyPairGenerator keyGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);
                keyGenerator.Initialize(builder);

                KeyPair keys = keyGenerator.GenerateKeyPair();
                Console.WriteLine("PUBLIC KEY: " + keys.Public.GetEncoded());
                //Console.WriteLine(keys.Public.GetEncoded());
                //Console.WriteLine("PRIVATE KEY: " + keys.Private.GetEncoded().ToString());
                //Console.WriteLine(keys.Private.GetEncoded());
                //SaveKeyIntoKeyStore(keys.Public.GetEncoded(), KeyPub);
                //SaveKeyIntoKeyStore(keys.Private.GetEncoded(), KeyPriv);
            }
            catch (Exception e)
            {
                Console.WriteLine("Not possible to generate the keys: " + e);
            }
        }

        private void SaveKeyIntoKeyStore(byte[] key, string alias)
        {
            keyStore.SetKeyEntry(alias, key, null);
        }

        public byte[] GetPublicKey()
        {
            return keyStore.GetKey(KeyPub, null).GetEncoded();
        }

        public void SaveHostPublicKey(byte[] key)
        {
            SaveKeyIntoKeyStore(key, KeycPub);
        }











        /*
        public string FreshMessage(string msg)
        {
            return JsonConvert.SerializeObject(new JsonFreshMessage(msg, GenerateNonce()));
        }

        public string JsonMessage(string msg)
        {
            string cipherText = RSAKeys.Encrypt(msg); //Public key from the host
            return JsonConvert.SerializeObject(new JsonCryptoDigestMessage(cipherText, DigestKey.Encode(cipherText)));
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);

            return JsonMessage(freshMessage);
        }

        public string DecodeAndDecryptMessage(string msg)
        {
            string jsonString = RSAKeys.Decrypt(AuthenticateMessage(msg));
            JsonFreshMessage jsonFreshMessage = JsonConvert.DeserializeObject<JsonFreshMessage>(jsonString);

            VerifyNonce(jsonFreshMessage.Nonce);

            return jsonFreshMessage.Message;
        }

        public void SetDeviceKey(string content)
        {
            string key = DecodeAndDecryptMessage(content);
            DevicePublicKey = JsonConvert.DeserializeObject<RSAParameters>(key);
        }

        public void VerifyNonce(string nonce)
        {
            if (!Nonce.Equals(nonce)) throw new Exception("Invalid nonce!\n");
        }

        public string AuthenticateMessage(string message)
        {
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            if (!jsonCryptoDigestMessage.Digest.Equals(DigestKey.Encode(message))) throw new Exception("Message was corrupted!\n");

            return jsonCryptoDigestMessage.Cryptotext;
        }*/
    }

    public class JsonFreshMessage
    {
        public string Message { get; set; }
        public string Nonce { get; set; }

        public JsonFreshMessage(string msg, string nonce)
        {
            Message = msg;
            Nonce = nonce;
        }
    }

    public class JsonCryptoDigestMessage
    {
        public string Cryptotext { get; set; }
        public string Digest { get; set; }

        public JsonCryptoDigestMessage(string cipher, string digest)
        {
            Cryptotext = cipher;
            Digest = digest;
        }
    }
}
