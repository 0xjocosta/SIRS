using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Java.Security;

namespace Key
{
    class RSAManager
    {
        private static string KeyPubFromHost = "KcPub";
        private static string KeyPub = "KsPub";
        private static string KeyPriv = "KsPri";

        public RSAParameters PrivKey { get; set; }
        public RSAParameters PubKey { get; set; }
        private RSACryptoServiceProvider Provider { get; set; }

        private KeyStore keyStore;

        public RSAManager() {
            //lets take a new CSP with a new 2048 bit rsa key pair
            Provider = new RSACryptoServiceProvider(2048);

            //how to get the private key
            PrivKey = Provider.ExportParameters(true);

            //and the public key ...
            PubKey = Provider.ExportParameters(false);

            keyStore = KeyStore.GetInstance("AndroidKeyStore");
        }

        public void SetPublicKey()
        {
            //keyStore.
        }

        private string KeyToString(RSAParameters key) {
            Console.WriteLine($"{Encoding.Unicode.GetString(key.Modulus)}");
            var sw = new StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            xs.Serialize(sw, key);

            return sw.ToString();
        }

        private void SaveKeyIntoKeystore(byte[] key, string alias)
        {
            try
            {
                /*keyStore.SetEntry(
                        Constants.KEY_ALIAS,
                        new KeyStore.SecretKeyEntry(new SecretKeySpec(bytes, 0, bytes.length, KeyProperties.KEY_ALGORITHM_AES)),
                new KeyProtection.Builder(KeyProperties.PURPOSE_ENCRYPT | KeyProperties.PURPOSE_DECRYPT)
                        .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                        .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                        .build());*/

                keyStore.SetKeyEntry(alias, key, null);
            }
            catch (Exception e)
            {
                Console.Write("Not possible to save the key in the KeyStore: " + e);
            }
        }

        /*private byte[] getKeyFromKeyStore(string alias)
        {
            return keyStore.Get;
        }*/

        public string GetPublicKeyString()
        {
            return KeyToString(PubKey);
        }

        public string GetPrivateKeyString() 
        {
            return KeyToString(PrivKey);
        }

        public RSAParameters KeyFromString(string key) {
            var sr = new StringReader(key);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            return (RSAParameters)xs.Deserialize(sr);
        }

        private string Encrypt(string plainTextData, RSAParameters key) {
            Provider = new RSACryptoServiceProvider();

            //for encryption, always handle bytes...
            byte[] bytesPlainTextData = Encoding.Unicode.GetBytes(plainTextData);
     
            Provider.ImportParameters(key);

            byte[] bytesCypherText = Provider.Encrypt(bytesPlainTextData, true);

            //we might want a string representation of our cypher text... base64 will do
            return Convert.ToBase64String(bytesCypherText);
        }

        public string Encrypt(string cypherText) {
            return Encrypt(cypherText, PubKey);
        }

        private string Decrypt(string cypherText, RSAParameters key) {
            byte[] bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            Provider = new RSACryptoServiceProvider();
            Provider.ImportParameters(key);

            byte[] bytesPlainTextData = Provider.Decrypt(bytesCypherText, true);

            //get our original plainText back...
            return Encoding.Unicode.GetString(bytesPlainTextData);
        }

        public string Decrypt(string cypherText)
        {
            return Decrypt(cypherText, PrivKey);
        }
    }
}
