using System;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Key
{
    class SecurityManager
    {
        //public HMACManager DigestKey { get; set; }
        public string Nonce { get; set; }
        public RSAManager RSAManager { get; set; }
        //public RSAParameters PcPublicKey { get; set;}

        private SecurityManagerHelper Helper;

        public SecurityManager(RSAParameters pcPubKey, byte[] Kdigest)
        {
            Helper = new SecurityManagerHelper();
            RSAManager RSAKeys = new RSAManager();
            RSAManager = RSAKeys;
            Helper.SavePairKey(RSAKeys.PubKey, RSAKeys.PrivKey);
            Helper.SavePcPublicKey(pcPubKey);
            Helper.SaveDigestKey(Kdigest);

            Nonce = Guid.NewGuid().ToString("N");
        }

        public SecurityManager()
        {
            Helper = new SecurityManagerHelper();
        }

        public RSAParameters GetPublicKey()
        {
            return Helper.GetPublicKey();
        }

        private string GenerateNonce() {
            Nonce = Guid.NewGuid().ToString("N");
            return Nonce;
        }

        public string FreshMessage(string msg)
        {
            return JsonConvert.SerializeObject(new JsonFreshMessage(msg, Nonce/*GenerateNonce()*/));
        }

        public string JsonMessage(string msg) {
            string cipherText = msg;
            RSAManager RSAKeys = new RSAManager(Helper.GetPcPublicKey());
            //string cipherText = RSAKeys.Encrypt(msg);
            HMACManager DigestKey = new HMACManager(Helper.GetDigestKey());
            return JsonConvert.SerializeObject(new JsonCryptoDigestMessage(cipherText, DigestKey.Encode(cipherText)));
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);

            return JsonMessage(freshMessage);
        }

        public string DecodeAndDecryptMessage(string msg)
        {
            string jsonString = msg;
            RSAManager RSAKeys = new RSAManager(Helper.GetPublicKey(), Helper.GetPrivateKey());
            //string jsonString = RSAKeys.Decrypt(AuthenticateMessage(msg));
            JsonFreshMessage jsonFreshMessage = JsonConvert.DeserializeObject<JsonFreshMessage>(AuthenticateMessage(jsonString));

            //VerifyNonce(jsonFreshMessage.Nonce);

            return jsonFreshMessage.Message;
        }

        public void VerifyNonce(string nonce)
        {
            if (!Nonce.Equals(nonce)) throw new Exception("Invalid nonce!\n");
        }

        public string AuthenticateMessage(string message)
        {
            HMACManager DigestKey = new HMACManager(Helper.GetDigestKey());
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            if (!jsonCryptoDigestMessage.Digest.Equals(DigestKey.Encode(jsonCryptoDigestMessage.Cryptotext))) throw new Exception("Message was corrupted!\n");

            return jsonCryptoDigestMessage.Cryptotext;
        }

        public string DecryptContentFromHost(string content)
        {
            RSAManager RSAKeys = new RSAManager(Helper.GetPublicKey(), Helper.GetPrivateKey());
            return RSAKeys.Decrypt(content);
        }
    }

    public class JsonRemote
    {
        public RSAParameters PublicKey { get; set; }
        public string ContentToDecipher { get; set; }
        public string DecipheredContent { get; set; }
        
        public JsonRemote() { }
    }

    public class JsonFreshMessage{
        public string Message { get; set; }
        public string Nonce { get; set; }

        public JsonFreshMessage(string msg, string nonce){
            Message = msg;
            Nonce = nonce;
        }
    }

    public class JsonCryptoDigestMessage {
        public string Cryptotext { get; set; }
        public string Digest { get; set; }

        public JsonCryptoDigestMessage(string cipher, string digest) {
            Cryptotext = cipher;
            Digest = digest;
        }
    }
}
