﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;

namespace HostLocker
{
    class UserDevice
    {
        public HMACManager DigestKey { get; set; }
        public string Nonce { get; set; }
        public AesManager SymmetricKey { get; set; }
        public BluetoothDeviceInfo BlDeviceInfo { get; set; }
        public RSAManager RSAKeys { get; set; }
        public RSAParameters DevicePublicKey { get; set; }
        public BluetoothClientWrapper BluetoothConnection { get; set; }
        public List<string> FilesList { get; set; }
        public string EncryptedSymmetricKey { get; set; }


        public UserDevice()
        {
            RSAKeys = new RSAManager();
            DigestKey = new HMACManager();
            Nonce = Guid.NewGuid().ToString("N");
        }

        public void AssociateDevice(BluetoothDeviceInfo device)
        {
            BlDeviceInfo = device;
        }

        public string GenerateNonce() {
            Nonce = Guid.NewGuid().ToString("N");
            return Nonce;
        }

        public void InitAesKey()
        {
            SymmetricKey = new AesManager();
            EncryptedSymmetricKey = RSAManager.Encrypt(JsonConvert.SerializeObject(SymmetricKey.Key), DevicePublicKey);
        }

        public string FreshMessage(string msg)
        {
            return JsonConvert.SerializeObject(new JsonFreshMessage(msg, GenerateNonce()));
        }

        public string JsonMessage(string msg) {
            //string cipherText = RSAKeys.Encrypt(msg, DevicePublicKey); UNCOMMENT THIS TO USE  FOR CONFIDENTIALITY
            return JsonConvert.SerializeObject(new JsonCryptoDigestMessage(msg, DigestKey.Encode(msg)));
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);

            return JsonMessage(freshMessage);
        }

        public string DecodeAndDecryptMessage(string msg)
        {
            //string jsonString = RSAKeys.Decrypt(AuthenticateMessage(msg));
            JsonFreshMessage jsonFreshMessage = JsonConvert.DeserializeObject<JsonFreshMessage>(AuthenticateMessage(msg));

            VerifyNonce(jsonFreshMessage.Nonce);

            return jsonFreshMessage.Message;
        }


        public void SetDeviceKey(RSAParameters content)
        {
            //string key = DecodeAndDecryptMessage(content);
            //DevicePublicKey = JsonConvert.DeserializeObject<RSAParameters>(content);
            DevicePublicKey = content;
        }

        public void VerifyNonce(string nonce)
        {
            //if (!Nonce.Equals(nonce)) throw new Exception("Invalid nonce!\n");
        }

        public string AuthenticateMessage(string message)
        {
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            //if (!jsonCryptoDigestMessage.Digest.Equals(DigestKey.Encode(jsonCryptoDigestMessage.Cryptotext))) throw new Exception("Message was corrupted!\n");

            return jsonCryptoDigestMessage.Cryptotext;
        }

        public JsonRemote DecryptedObjReceived(string content)
        {
            string decryptedMsg = DecodeAndDecryptMessage(content);
            return JsonConvert.DeserializeObject<JsonRemote>(decryptedMsg);
        }

        public string PrepareDecryptRequest()
        {
            //Object to be send to the smartphone
            JsonRemote jsonRemote = new JsonRemote();

            //set info to be decipher in smartphone
            jsonRemote.ContentToDecipher = EncryptedSymmetricKey;

            //encrypt and encode the serialize object
            return EncryptAndEncodeMessage(JsonConvert.SerializeObject(jsonRemote));
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
