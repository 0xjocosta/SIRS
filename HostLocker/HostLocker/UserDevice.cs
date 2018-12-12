using System;
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
        public HMACManager hmacManager { get; set; }
        public long Nonce { get; set; }
        public AesManager aesManager { get; set; }
        public RSAManager rsaManager { get; set; }
        public RSAParameters DevicePublicKey { get; set; }
        public BluetoothClientWrapper BluetoothConnection { get; set; }
        public List<string> FilesList { get; set; } = new List<string>();
        public string EncryptedSymmetricKey { get; set; }
        public AesManager ConnAesManager { get; set; } = new AesManager();

        public UserDevice()
        {
            rsaManager = new RSAManager();
            hmacManager = new HMACManager();
            Nonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public UserDevice(UserData user)
        {
            rsaManager = new RSAManager(user.UserPubKey.RSAParameters, user.UserPrivKey.RSAParameters);
            aesManager = new AesManager();
            DevicePublicKey = user.DevicePublicKey.RSAParameters;
            FilesList = user.Files;
            hmacManager = new HMACManager(user.UserSecretKey);
            GenerateNonce();
            EncryptedSymmetricKey = user.EncryptedUserAesKey;
        }

        public long GenerateNonce() {
            Nonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Nonce;
        }

        public void InitAesManager()
        {
            aesManager = new AesManager();
            aesManager.InitKey();
            EncryptedSymmetricKey = rsaManager.Encrypt(JsonConvert.SerializeObject(aesManager.Key), DevicePublicKey);
        }


        public string FreshMessage(string msg)
        {
            return JsonConvert.SerializeObject(new JsonFreshMessage(msg, GenerateNonce()));
        }

        public string JsonMessage(string msg) {
            //Refresh every message the keys 
            ConnAesManager.InitKey();

            //Cipher content with the symmetric key
            byte[] bytes = ConnAesManager.EncryptStringToBytes_Aes(msg);

            //Cipher the symmetric key with pub key
            KeyDecipher keyDecipher = new KeyDecipher(ConnAesManager.Key, ConnAesManager.InitVect);
            string KeyDecipher = JsonConvert.SerializeObject(keyDecipher);
            string cipheredKey = rsaManager.Encrypt(KeyDecipher, DevicePublicKey);

            //The cipherText and cipherKey
            Message message = new Message(bytes, cipheredKey);

            //Digest the message
            return JsonConvert.SerializeObject(new JsonCryptoDigestMessage(JsonConvert.SerializeObject(message), hmacManager.Encode(JsonConvert.SerializeObject(message))));
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);

            return JsonMessage(freshMessage);
        }

        public string DecodeAndDecryptMessage(string msg)
        {
            //string jsonString = rsaManager.Decrypt(AuthenticateMessage(msg));
            Message messageRecv = AuthenticateMessage(msg);

            string decryptedMessage = rsaManager.Decrypt(messageRecv.KeyToDecipher);

            KeyDecipher key = JsonConvert.DeserializeObject<KeyDecipher>(decryptedMessage);

            ConnAesManager.SetKey(key.Key, key.IV);

            string jsonString = ConnAesManager.DecryptStringFromBytes_Aes(messageRecv.Cryptotext);

            JsonFreshMessage jsonFreshMessage = JsonConvert.DeserializeObject<JsonFreshMessage>(jsonString);

            VerifyNonce(jsonFreshMessage.Nonce);

            return jsonFreshMessage.Message;
        }

        public void VerifyNonce(long nonce)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if ( now -10 > nonce || nonce > now + 10 ) throw new Exception("Invalid nonce!\n");
        }

        public Message AuthenticateMessage(string message)
        {
            JsonCryptoDigestMessage jsonCryptoDigestMessage = JsonConvert.DeserializeObject<JsonCryptoDigestMessage>(message);
            if (!jsonCryptoDigestMessage.Digest.Equals(hmacManager.Encode(jsonCryptoDigestMessage.Message))) throw new Exception("Message was corrupted!\n");

            return JsonConvert.DeserializeObject<Message>(jsonCryptoDigestMessage.Message);
        }

        public JsonRemote DecryptedObjReceived(string content)
        {
            string decryptedMsg = DecodeAndDecryptMessage(content);
            return JsonConvert.DeserializeObject<JsonRemote>(decryptedMsg);
        }

        public string PrepareDecryptRequest(string encryptedSymmetricKey) {
            //Object to be send to the smartphone
            JsonRemote jsonRemote = new JsonRemote();

            //set info to be decipher in smartphone
            jsonRemote.ContentToDecipher = encryptedSymmetricKey;

            //encrypt and encode the serialize object
            return EncryptAndEncodeMessage(JsonConvert.SerializeObject(jsonRemote));
        }
    }

    public class JsonRemote
    {
        public RSAParametersSerializable PublicKey { get; set; }
        public string ContentToDecipher { get; set; }
        public string DecipheredContent { get; set; }

        public JsonRemote() { }
    }

    public class KeyDecipher {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public KeyDecipher(byte[] key, byte[] iv) {
            Key = key;
            IV = iv;
        }
    }

    public class Message {
        public byte[] Cryptotext { get; set; }
        public string KeyToDecipher { get; set; }

        public Message(byte[] cryptoText, string keyDecipher) {
            Cryptotext = cryptoText;
            KeyToDecipher = keyDecipher;
        }
    }

    public class JsonFreshMessage {
        public string Message { get; set; }
        public long Nonce { get; set; }

        public JsonFreshMessage(string msg, long nonce) {
            Message = msg;
            Nonce = nonce;
        }
    }

    public class JsonCryptoDigestMessage
    {
        public string Message { get; set; }
        public string Digest { get; set; }

        public JsonCryptoDigestMessage(string msg, string digest) {
            Message = msg;
            Digest = digest;
        }
    }
}
