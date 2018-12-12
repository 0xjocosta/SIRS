using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Android.Content;
using Android.Preferences;
using Newtonsoft.Json;

namespace Key
{

    public class SecurityManagerHelper
    {
        readonly string KeycPub = "KcPub";
        readonly string KeyDigest = "Kd";
        readonly string KeysPub = "KsPub";
        readonly string KeysPriv = "KsPri";

        private Context mContext;

        public SecurityManagerHelper()
        {
            mContext = Android.App.Application.Context;
        }

        public void SavePcPublicKey(RSAParameters pubKey)
        {
            RSAParametersSerializable rsaPub = new RSAParametersSerializable(pubKey);
            string pub = JsonConvert.SerializeObject(rsaPub);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(KeycPub, pub);
            editor.Apply();
        }

        public void SavePairKey(RSAParameters pub, RSAParameters priv)
        {
            RSAParametersSerializable rsaPub = new RSAParametersSerializable(pub);
            string pubstr = JsonConvert.SerializeObject(rsaPub);
            RSAParametersSerializable rsaPriv = new RSAParametersSerializable(priv);
            string privstr = JsonConvert.SerializeObject(rsaPriv);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(KeysPub, pubstr);
            editor.PutString(KeysPriv, privstr);
            editor.Apply();
        }

        public void SaveDigestKey(byte[] Kdigest)
        {
            string digest = JsonConvert.SerializeObject(Kdigest);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(KeyDigest, digest);
            editor.Apply();
        }

        public RSAParameters GetPcPublicKey()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            string pub = prefs.GetString(KeycPub, "");
            RSAParametersSerializable rsaPub = JsonConvert.DeserializeObject<RSAParametersSerializable>(pub);
            return rsaPub.RSAParameters;
        }

        public RSAParametersSerializable GetPublicKey()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            string pub = prefs.GetString(KeysPub, "");
            RSAParametersSerializable rsaPub = JsonConvert.DeserializeObject<RSAParametersSerializable>(pub);
            return rsaPub;
        }

        public RSAParameters GetPrivateKey()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            string priv = prefs.GetString(KeysPriv, "");
            RSAParametersSerializable rsaPriv = JsonConvert.DeserializeObject<RSAParametersSerializable>(priv);
            return rsaPriv.RSAParameters;
        }

        public byte[] GetDigestKey()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            string digest = prefs.GetString(KeyDigest, "");
            return JsonConvert.DeserializeObject<byte[]>(digest);
        }
    }
}
