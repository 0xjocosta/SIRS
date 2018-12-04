using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using InTheHand.Net.Sockets;

namespace HostLocker
{
    class UserDevice
    {
        public HMACManger DigestKey { get; set; }
        public string Nonce { get; set; }
        public AesManager SymmetricKey { get; set; }
        public BluetoothDeviceInfo BlDeviceInfo { get; set; }
        public RSAManager RSAKeys{ get; set; }
        public RSAParameters DevicePublicKey { get; set;}

        public UserDevice()
        {
            RSAKeys = new RSAManager();
            DigestKey = new HMACManger();
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
        }

        public string FreshMessage(string msg)
        {
            return $"{msg}|{GenerateNonce()}";
        }

        public string EncryptAndEncodeMessage(string msg)
        {
            string freshMessage = FreshMessage(msg);
            string cipherText = RSAKeys.Encrypt(msg, DevicePublicKey);
            string digest = DigestKey.Encode(cipherText);

            return $"{cipherText}|{digest}";
        }

        public string DecodeAndDecryptMessage(string msg)
        {
            AuthenticateMessage(ref msg);
            string plainText = RSAKeys.Decrypt(msg);
            VerifyNonce(ParseKey(ref plainText));

            return plainText;
        }

        public void SetDeviceKey(string content)
        {
            string copy = content;

            VerifyNonce(ParseKey(ref copy));

            DevicePublicKey = RSAKeys.KeyFromString(ParseKey(ref copy, true));
        }

        public void VerifyNonce(string nonce)
        {
            if (!Nonce.Equals(nonce)) throw new Exception("Invalid nonce!\n");
        }

        public void AuthenticateMessage(ref string message)
        {
            string digest = ParseKey(ref message);
            if (!digest.Equals(DigestKey.Encode(message))) throw new Exception("Message was corrupted!\n");
        }

        public static string ParseKey(ref string content, bool last = false) {
            Regex regex = new Regex(@"^(\w+)\|(\w+)|(\w+)$");
            Match match = regex.Match(content);
            if (match.Success)
            {
                content = (last ? match.Groups[0].Value : match.Groups[1].Value);
                return (last ? match.Groups[3].Value : match.Groups[2].Value);
            }
            throw new Exception("Invalid string content!\n.");
        }
    }
}
