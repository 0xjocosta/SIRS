
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

namespace Key
{
    [Activity(Label = "ConnectionActivity")]
    public class ConnectionActivity : Activity
    {
        Button btnstart;
        Button btnstop;
        TextView txtvstart;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Connection);

            btnstart = FindViewById<Button>(Resource.Id.startButton);
            btnstart.Click += Btnstart_Click;

            btnstop = FindViewById<Button>(Resource.Id.stopButton);
            btnstop.Click += Btnstop_Click;

            //Text
            txtvstart = FindViewById<TextView>(Resource.Id.pairedDevices);

            Button backToMain = FindViewById<Button>(Resource.Id.backToMainFromConnection);
            backToMain.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                //intent.PutStringArrayListExtra("phone_numbers", phoneNumbers);
                StartActivity(intent);
            };
            // Create your application here
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
                txtvstart.Text = "Name <<" + AdapterName + ">>" + "Address <<" + AdapterAddress + ">>" + "BoundDevices <<" + AdapterBoundDevices + ">>" + "State <<" + AdapterState + ">>";

                ConnectingToDevice(devBluetooth);
            }
            else
            {
                StartActivityForResult(new Intent(Android.Bluetooth.BluetoothAdapter.ActionRequestEnable), 0);
            }
        }

        private void ConnectingToDevice(BluetoothDevice dev)
        {


        }

        private void Btnstop_Click(object sender, EventArgs e)
        {
            BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
            if (defaultAdapter.IsEnabled)
            {
                defaultAdapter.Disable();
                txtvstart.Text = "---";
            }

        }

    }
}
