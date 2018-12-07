using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Java.Security;
using Android.Security.Keystore;
using Javax.Crypto;
using Javax.Crypto.Spec;
using Android.Util;
using System.Text;

namespace Key
{
    public class SecurityManager
    {
        private static readonly string KEYSTORE_NAME = "AndroidKeyStore";
        private static readonly string KEY_ALIAS = "PairKeys";
        private static readonly int KEY_SIZE = 2048;
        private KeyStore keyStore;

        private RSAManager rsaManager;
        private string Nonce { get; set; }

        //variables to save
        private static readonly string nonce = "Nonce";
        private static readonly string KeycPub = "KcPub";
        private static readonly string KeyDigest = "Kd";

        private static readonly string KeyPub = "KsPub";
        private static readonly string KeyPriv = "KsPri";

        private IPrivateKey privKey;
        private KeyPair keys;
        public SecurityManager()
        {
            keyStore = KeyStore.GetInstance(KEYSTORE_NAME);     
            keyStore.Load(null);
            GenerateNonce();
            GeneratePairKeys();
            /*rsaManager = new RSAManager();
            Console.WriteLine("PUBLICCCCCCCCCCCCCCCCCCCCCCC");
            Console.WriteLine(rsaManager.PubKey);
            Console.WriteLine("PRIVATEEEEEEEEEEEEEEEEEEEEEEEE");
            Console.WriteLine(rsaManager.PrivKey);*/
        }

        public string GenerateNonce()
        {
            Nonce = Guid.NewGuid().ToString("N");
            return Nonce;
        }

        private void GeneratePairKeys()
        {
            try
            {
                // Get the KeyPairGenerator instance for RSA
                KeyPairGenerator keyPairGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KEYSTORE_NAME);

                // The KeyGenParameterSpec is how parameters for your key are passed to the generator.
                /*keyPairGenerator.Initialize(new KeyGenParameterSpec.Builder(
                    KEY_ALIAS, KeyStorePurpose.Decrypt | KeyStorePurpose.Encrypt)
                    .SetBlockModes("ECB")
                    .SetEncryptionPaddings("NOPADDING")
                    .SetKeySize(KEY_SIZE)
                    .Build());
                */

                keyPairGenerator.Initialize(2048);
                //Generate keys
                keys = keyPairGenerator.GenerateKeyPair();
                privKey = keyPairGenerator.GenerateKeyPair().Private;
            }
            catch (Exception e)
            {
                Console.WriteLine("Not possible to generate the keys: " + e);
            }

            Console.WriteLine("Public in generator");
            Console.WriteLine(keys.Public);
        }

        private IKey GetPrivKey()
        {
            KeyStore.IEntry entry = keyStore.GetEntry(KEY_ALIAS, null);
            return ((KeyStore.PrivateKeyEntry)entry).PrivateKey;
        }

        private Java.Security.Cert.Certificate GetMyCertificate()
        {
            return keyStore.GetCertificate(KEY_ALIAS);
        }

        public byte[] GetMyPublicKey()
        {
            return GetMyCertificate().PublicKey.GetEncoded();
        }

        private void SaveKeyIntoKeyStore(byte[] key, string alias)
        {
            keyStore.SetKeyEntry(alias, key, null);
        }

        public void SaveHostPublicKey(byte[] key)
        {
            SaveKeyIntoKeyStore(key, KeycPub);
        }

        public byte[] Encrypt(byte[] content)
        {
            Cipher encryptCipher = Cipher.GetInstance("RSA/ECB/NOPADDING");
            encryptCipher.Init(Javax.Crypto.CipherMode.EncryptMode, keys.Public);

            byte[] cipherText = encryptCipher.DoFinal(content);

            return cipherText;
            //return Base64.EncodeToString(cipherText, Base64Flags.Default);
        }

        public string Decrypt(byte[] content)
        {
            Cipher decriptCipher = Cipher.GetInstance("RSA/ECB/NOPADDING");
            decriptCipher.Init(Javax.Crypto.CipherMode.DecryptMode, keys.Private);

            return Encoding.ASCII.GetString(content);
        }
    }
}
