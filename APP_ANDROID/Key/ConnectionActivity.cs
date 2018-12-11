using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Java.Util;
using System.IO;
using Newtonsoft.Json;
using Android.Security;

namespace Key
{
    [Activity(Label = "ConnectionActivity")]
    public class ConnectionActivity : Activity
    {
        //Connection Manager
        ConnectionManager connectionManager;

        bool CONNECTED = false;

        Button btnstart;
        Button btnstop;

        //SOCKET INFO
        BluetoothSocket mmSocket = null;
        Stream inputStream;
        Stream outputStream;

        //DEBUG INFO
        TextView txtDebug;

        Dictionary<string, string> dict = new Dictionary<string, string>() {
            {"Nonce", ""},
            {"KcPub", ""},
            {"Kd", ""},
            {"KsPub", "KeySuckMI"},
            {"KsPri", "KsPriv"}
        };

        //variables to save
        string nonce = "Nonce";
        string KeycPub = "KcPub";
        string KeyDigest = "Kd";

        string KeysPub = "KsPub";
        string KeysPriv = "KsPri";

        //QRCODE
        string qrCodeInfo = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Connection);

            btnstart = FindViewById<Button>(Resource.Id.startButton);
            btnstart.Click += Btnstart_Click;

            btnstop = FindViewById<Button>(Resource.Id.stopButton);
            btnstop.Click += Btnstop_Click;

            //Debug
            txtDebug = FindViewById<TextView>(Resource.Id.debugLog);

            Button backToMain = FindViewById<Button>(Resource.Id.backToMainFromConnection);
            backToMain.Click += (sender, e) =>
            {
                BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
                if (defaultAdapter.IsEnabled)
                {
                    defaultAdapter.Disable();
                }
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            };
        }

        private void Btnstop_Click(object sender, EventArgs e)
        {
            BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
            if (defaultAdapter.IsEnabled)
            {
                defaultAdapter.Disable();
            }

        }

        private void Btnstart_Click(object sender, EventArgs e)
        {
            BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
            BluetoothDevice devBluetooth = null;
            string AdapterAddress, AdapterName, AdapterBoundDevices = String.Empty;
            State AdapterState;
            if (defaultAdapter.IsEnabled)
            {
                AdapterAddress = defaultAdapter.Address;
                AdapterName = defaultAdapter.Name;
                var bd = defaultAdapter.BondedDevices;
                foreach (var dev in bd)
                {
                    if (!String.IsNullOrEmpty(AdapterBoundDevices))
                    {
                        AdapterBoundDevices += ",";
                    }
                    AdapterBoundDevices += dev.Name;
                    devBluetooth = dev;
                }
                AdapterState = defaultAdapter.State;

                connectionManager = new ConnectionManager(devBluetooth, txtDebug);

                string lastPage = Intent.GetStringExtra("LAST_PAGE");
                if (lastPage == "REGISTER")
                {
                    qrCodeInfo = Intent.GetStringExtra("QRCodeInformation");
                    Console.WriteLine(qrCodeInfo);
                    SetInformationQrCode(qrCodeInfo);

                    connectionManager.SetConnectionWithInfo(qrCodeInfo);
                }
                else
                {
                    connectionManager.SetConnection();
                }
            }
            else
            {
                StartActivityForResult(new Intent(Android.Bluetooth.BluetoothAdapter.ActionRequestEnable), 0);
            }
        }


        private void SetInformationQrCode(string qrcode)
        {
            dynamic dicKeys = JsonConvert.DeserializeObject(qrcode);
            dict[nonce] = dicKeys[nonce].Value;
            dict[KeycPub] = dicKeys[KeycPub].Value;
            dict[KeyDigest] = dicKeys[KeyDigest].Value;
        }



        private string SerializeJsonToString(string[] strings)
        {
            Dictionary<string, string> json = new Dictionary<string, string>();
            foreach (string str in strings)
            {
                json.Add(str, dict[str]);
            }

            return JsonConvert.SerializeObject(json);
        }
    }
}