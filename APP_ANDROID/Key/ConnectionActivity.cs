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
        bool CONNECTED = false;

        Button btnstart;
        Button btnstop;
        BluetoothSocket mmSocket = null;
        Stream inputStream;
        Stream outputStream;

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

        private string SerializeJsonToString(string[] strings)
        {
            Dictionary<string, string> json = new Dictionary<string, string>();
            foreach (string str in strings) {
                json.Add(str,dict[str]);
            }

            return JsonConvert.SerializeObject(json);
        }

        private void SetInformationQrCode(string qrcode) {
            dynamic dicKeys = JsonConvert.DeserializeObject(qrcode);
            dict[nonce] = dicKeys[nonce].Value;
            dict[KeycPub] = dicKeys[KeycPub].Value;
            dict[KeyDigest] = dicKeys[KeyDigest].Value;
        }

        private void SendPubKey() {
            string[] strings = { KeysPub, nonce };
            string infoToSend = SerializeJsonToString(strings);
            WriteToSocket(infoToSend);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //string FileName = "qrcode.txt";
            base.OnCreate(savedInstanceState);
            string extra = Intent.GetStringExtra("QRCodeInformation");

            if (extra != "" && extra != "Please focus Camera to QR Code")
            {
                qrCodeInfo = extra;
                //SaveCountAsync(qrCodeInfo, FileName);
                Console.WriteLine(extra);
                SetInformationQrCode(qrCodeInfo);
            }

            //qrCodeInfo = ReadFile(FileName);
            Console.WriteLine(qrCodeInfo);
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
                CancelConnection();
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            };
            // Create your application here
        }

        private void Btnstop_Click(object sender, EventArgs e)
        {
            CancelConnection();
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
                ConnectingToDevice(devBluetooth);
            }
            else
            {
                StartActivityForResult(new Intent(Android.Bluetooth.BluetoothAdapter.ActionRequestEnable), 0);
            }
        }

        private void ConnectingToDevice(BluetoothDevice dev)
        {
            UUID myUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            BluetoothSocket tmp = null;
            try
            {
                // MY_UUID is the app's UUID string, also used by the server code
                tmp = dev.CreateRfcommSocketToServiceRecord(myUUID);
            }
            catch (System.IO.IOException) {
                Console.WriteLine("FAILED TO CREATE SOCKET");
            }
            mmSocket = tmp;
            BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
            defaultAdapter.CancelDiscovery();

            try
            {
                // Connect the device through the socket. This will block
                // until it succeeds or throws an exception
                mmSocket.Connect();
                CONNECTED = true;
            }
            catch (IOException)
            {
                Console.WriteLine("FAILED TO CONNECT");
                // Unable to connect; close the socket and get out
                try
                {
                    mmSocket.Close();
                    CONNECTED = false;
                }
                catch (IOException) {
                    Console.WriteLine("FAILED TO CLOSE");
                }
                return;
            }

            // Do work to manage the connection (in a separate thread)
            ManageConnectedSocket();
        }

        /** Will cancel an in-progress connection, and close the socket */
        public void CancelConnection()
        {
            if (CONNECTED == false) {
                return;
            }
            try
            {
                mmSocket.Close();
            }
            catch (IOException) { }
        }

        private void ManageConnectedSocket()
        {
            Stream tmpIn = null;
            Stream tmpOut = null;
            try
            {
                tmpIn = mmSocket.InputStream;
                tmpOut = mmSocket.OutputStream;
            }
            catch (IOException) { }

            inputStream = tmpIn;
            outputStream = tmpOut;

            SendPubKey();
            ListeningFromSocketAsync();

         }

        private async void ListeningFromSocketAsync()
        {
            byte[] buffer = new byte[1024];  // buffer store for the stream
            //int bytes; // bytes returned from read()

            // Keep listening to the InputStream until an exception occurs
            while (true)
            {
                try
                {
                    // Read from the InputStream
                    int x = await inputStream.ReadAsync(buffer, 0, 1024);
                    string str = Encoding.ASCII.GetString(buffer);
                    txtDebug.Text += "Receiving: " + str + '\n';
                    // Send the obtained bytes to the UI activity
                    //mHandler.obtainMessage(MESSAGE_READ, bytes, -1, buffer).sendToTarget();
                }
                catch (IOException)
                {
                    break;
                }
            }
        }

        /* Call this from the main activity to send data to the remote device */
        public void WriteToSocket(string str)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str + '|');
                outputStream.Write(bytes, 0, bytes.Length);

                txtDebug.Text += "Sending: " + str + '\n';
            }
            catch (IOException) { }
        }

        /*
        public async void SaveCountAsync(string str, string FileName)
        {
            var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), FileName);
            using (var writer = File.CreateText(backingFile))
            {
                await writer.WriteLineAsync(str);
            }
        }

        public string ReadFile(string FileName)
        {
            var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), FileName);
            string output = "";

            if (backingFile == null || !File.Exists(backingFile))
            {
                return output;
            }

            using (var reader = new StreamReader(backingFile, true))
            {
                output = reader.ReadToEnd();
            }

            return output;
        }*/
    }
}