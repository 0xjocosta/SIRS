using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace HostLocker
{
    public class RSAManager
    {
        public RSAParameters PrivKey { get; set; }
        public RSAParameters PubKey { get; set; }
        public static RSACryptoServiceProvider Provider { get; set; }

        public RSAManager() {
            //lets take a new CSP with a new 2048 bit rsa key pair
            Provider = new RSACryptoServiceProvider(2048);

            //how to get the private key
            PrivKey = Provider.ExportParameters(true);

            //and the public key ...
            PubKey = Provider.ExportParameters(false);
        }

        public RSAManager(RSAParameters pub, RSAParameters priv)
        {
            PubKey = pub;
            PrivKey = priv;
        }

        public string KeyToString(RSAParameters key) {
            Console.WriteLine($"{Encoding.ASCII.GetString(key.Modulus)}");
            var sw = new StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            xs.Serialize(sw, key);

            return sw.ToString();
        }

        public string GetPublicKeyString()
        {
            return KeyToString(PubKey);
        }

        public string GetPrivateKeyString() {
            return KeyToString(PrivKey);
        }

        public RSAParameters KeyFromString(string key) {
            var sr = new StringReader(key);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            return (RSAParameters)xs.Deserialize(sr);
        }

        public static string Encrypt(string plainTextData, RSAParameters key) {
            if (Provider == null)
            {
                Provider = new RSACryptoServiceProvider();
            }

            //for encryption, always handle bytes...
            byte[] bytesPlainTextData = Encoding.ASCII.GetBytes(plainTextData);
     
            Provider.ImportParameters(key);

            byte[] bytesCypherText = Provider.Encrypt(bytesPlainTextData, true);

            //we might want a string representation of our cypher text... base64 will do
            return Convert.ToBase64String(bytesCypherText);
        }
        public string Encrypt(string cypherText) {
            return Encrypt(cypherText, PubKey);
        }

        public string Decrypt(string cypherText, RSAParameters key) {
            byte[] bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            if (Provider == null) {
                Provider = new RSACryptoServiceProvider();
            }
            Provider.ImportParameters(key);

            byte[] bytesPlainTextData = Provider.Decrypt(bytesCypherText, true);

            //get our original plainText back...
            return Encoding.ASCII.GetString(bytesPlainTextData);
        }

        public string Decrypt(string cypherText)
        {
            return Decrypt(cypherText, PrivKey);
        }
    }
}
