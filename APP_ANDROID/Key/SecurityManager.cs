using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Key
{
    class SecurityManager
    {
        private SecurityManagerHelper Helper;

        public SecurityManager(RSAParameters pcPubKey, byte[] Kdigest)
        {
            Helper = new SecurityManagerHelper();
            RSAManager RSAKeys = new RSAManager();
            Helper.SavePairKey(RSAKeys.PubKey, RSAKeys.PrivKey);
            Helper.SavePcPublicKey(pcPubKey);
            Helper.SaveDigestKey(Kdigest);
        }

        public SecurityManager()
        {
            Helper = new SecurityManagerHelper();
        }

        public RSAParametersSerializable GetPublicKey()
        {
            return Helper.GetPublicKey();
        }

        public long GenerateNonce()
        {
            long nonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return nonce;
        }

        public string FreshMessage(string msg)
        {
            return JsonConvert.SerializeObject(new JsonFreshMessage(msg, GenerateNonce()));
        }

        public string JsonMessage(string msg) {
            RSAManager RSAKeys = new RSAManager(Helper.GetPcPublicKey());
            AesManager aesManager = new AesManager();
            aesManager.InitKey();
            //Cipher content with the symmetric key
            byte[] bytes = aesManager.EncryptStringToBytes_Aes(msg);
            //string cipherText = JsonConvert.SerializeObject(bytes);

            //Cipher the symmetric key with pub key
            KeyDecipher keyDecipher = new KeyDecipher(aesManager.Key, aesManager.InitVect);
            string KeyDecipher = JsonConvert.SerializeObject(keyDecipher);
            string cipheredKey = RSAKeys.Encrypt(KeyDecipher);

            //The cipherText and cipherKey
            Message message = new Message(bytes, cipheredKey);
            string Message = JsonConvert.SerializeObject(message);

            //Digest the message
            HMACManager DigestKey = new HMACManager(Helper.GetDigestKey());
            return JsonConvert.SerializeObject(new JsonCryptoDigestMessage(Message, DigestKey.Encode(Message)));
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);

            return JsonMessage(freshMessage);
        }

        public string DecodeAndDecryptMessage(string msg)
        {

            RSAManager RSAKeys = new RSAManager(Helper.GetPublicKey().RSAParameters, Helper.GetPrivateKey());
            Message message = AuthenticateMessage(msg);

            //Decipher symm key to decipher the content
            string keyToDecipher = RSAKeys.Decrypt(message.KeyToDecipher);
            KeyDecipher keyDecipher = JsonConvert.DeserializeObject<KeyDecipher>(keyToDecipher);

            //Decipher content
            AesManager aesManager = new AesManager();
            aesManager.SetKey(keyDecipher.Key, keyDecipher.IV);

            Console.WriteLine(Encoding.ASCII.GetString(message.Cryptotext));
            string Content = aesManager.DecryptStringFromBytes_Aes(message.Cryptotext);

            JsonFreshMessage jsonFreshMessage = JsonConvert.DeserializeObject<JsonFreshMessage>(Content);

            VerifyNonce(jsonFreshMessage.Nonce);

            return jsonFreshMessage.Message;
        }

        public void VerifyNonce(long nonce)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - 10 > nonce || nonce > now + 10) throw new Exception("Invalid nonce!\n");
        }

        public Message AuthenticateMessage(string message)
        {
            HMACManager DigestKey = new HMACManager(Helper.GetDigestKey());
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            if (!jsonCryptoDigestMessage.Digest.Equals(DigestKey.Encode(jsonCryptoDigestMessage.Message))) throw new Exception("Message was corrupted!\n");

            return JsonConvert.DeserializeObject<Message>(jsonCryptoDigestMessage.Message);
        }

        public string DecryptContentFromHost(string content)
        {
            RSAManager RSAKeys = new RSAManager(Helper.GetPublicKey().RSAParameters, Helper.GetPrivateKey());
            return RSAKeys.Decrypt(content);
        }
    }

    public class JsonRemote
    {
        public RSAParametersSerializable PublicKey { get; set; }
        public string ContentToDecipher { get; set; }
        public string DecipheredContent { get; set; }
        
        public JsonRemote() { }
    }

    public class KeyDecipher
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public KeyDecipher(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }
    }

    public class Message
    {
        public byte[] Cryptotext { get; set; }
        public string KeyToDecipher { get; set; }

        public Message(byte[] cryptoText, string keyDecipher)
        {
            Cryptotext = cryptoText;
            KeyToDecipher = keyDecipher;
        }
    }

    public class JsonFreshMessage{
        public string Message { get; set; }
        public long Nonce { get; set; }

        public JsonFreshMessage(string msg, long nonce){
            Message = msg;
            Nonce = nonce;
        }
    }

    public class JsonCryptoDigestMessage {
        public string Message { get; set; }
        public string Digest { get; set; }

        public JsonCryptoDigestMessage(string message, string digest) {
            Message = message;
            Digest = digest;
        }
    }
}
