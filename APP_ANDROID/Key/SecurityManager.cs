﻿using System;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Key
{
    class SecurityManager
    {
        public HMACManager DigestKey { get; set; }
        public string Nonce { get; set; }
        public RSAManager RSAKeys { get; set; }
        public RSAParameters PcPublicKey { get; set;}

        public SecurityManager()
        {
            RSAKeys = new RSAManager();
            Nonce = Guid.NewGuid().ToString("N");
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
            //string cipherText = RSAKeys.Encrypt(msg, PcPublicKey);
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
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            if (!jsonCryptoDigestMessage.Digest.Equals(DigestKey.Encode(jsonCryptoDigestMessage.Cryptotext))) throw new Exception("Message was corrupted!\n");

            return jsonCryptoDigestMessage.Cryptotext;
        }

        public void SetDigestKey(byte[] key)
        {
            DigestKey = new HMACManager(key);
        }

        public void SetPcPublicKey(RSAParameters content)
        {
            PcPublicKey = content;
        }

        public string DecryptContentFromHost(string content)
        {
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
