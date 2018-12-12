using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HostLocker {
    class BluetoothManager {
        private bool _beginConnect;
        private BluetoothClient _bluetoothClient;
        public BluetoothClientWrapper BluetoothRemoteClient { get; set; } = null;
        public BluetoothListener bl { get; set; }
        private bool serverStarted = false;
        public string nonce;

        BluetoothAddress LOCAL_MAC;

        // mac is mac address of local bluetooth device
        BluetoothEndPoint localEndpoint;
        // client is used to manage connections
        BluetoothClient localClient;
        // component is used to manage device discovery
        BluetoothComponent localComponent;
        // List of Devices Found
        public List<BluetoothDeviceInfo> deviceList = new List<BluetoothDeviceInfo>();
        RegisterWindow _window;
        List<Device> devicesWrapper = new List<Device>();

        public BluetoothManager() {
            LOCAL_MAC = GetBTMacAddress();
            if (LOCAL_MAC != null) {
                localEndpoint = new BluetoothEndPoint(LOCAL_MAC, BluetoothService.SerialPort);
                localClient = new BluetoothClient(localEndpoint);
                /* Scan for devices when instantiat bl manager
                localComponent = new BluetoothComponent(localClient);
                // async methods, can be done synchronously too
                localComponent.DiscoverDevicesAsync(255, true, true, true, false, null);
                localComponent.DiscoverDevicesProgress += Scan;
                localComponent.DiscoverDevicesComplete += Scan_Complete;                
                window.pb.Visibility = Visibility.Visible;
                _window.listen_btn.Visibility = Visibility.Hidden;
                _window.btn_find.Visibility = Visibility.Hidden;
                */
            }
            else {
                throw new Exception("No radio hardware or unsupported software stack");
            }
        }

        public BluetoothDeviceInfo GetDevicesByAddress(BluetoothAddress address) {
            foreach (BluetoothDeviceInfo device in deviceList) {
                if (device.DeviceAddress.Equals(address))
                    return device;
            }
            return null;
        }

        public static BluetoothAddress GetBTMacAddress() {

            BluetoothRadio myRadio = BluetoothRadio.PrimaryRadio;
            if (myRadio == null) {
                return null;
            }

            return myRadio.LocalAddress;
        }

        public void Scan(object sender, DiscoverDevicesEventArgs e) {
            // log and save all found devices

            for (int i = 0; i < e.Devices.Length; i++) {
                this.deviceList.Add(e.Devices[i]);

                if (e.Devices[i].Remembered) {
                    Console.WriteLine(e.Devices[i].DeviceName + " (" + e.Devices[i].DeviceAddress + "): Device is known");
                }
                else {
                    devicesWrapper.Add(new Device(e.Devices[i]));
                    Console.WriteLine(e.Devices[i].DeviceName + " (" + e.Devices[i].DeviceAddress + "): Device is unknown");
                }
            }
        }

        private void Scan_Complete(object sender, DiscoverDevicesEventArgs e) {
            Console.WriteLine("Devices discovered: " + e.Devices);
            /*
            _window.pb.Visibility = Visibility.Hidden;
            _window.device_list.ItemsSource = devicesWrapper;
            _window.listen_btn.Visibility = Visibility.Visible;
            _window.btn_find.Visibility = Visibility.Visible;
            */
        }

        public bool Pair(BluetoothAddress address, string DEVICE_PIN) {
            return BluetoothSecurity.PairRequest(address, DEVICE_PIN);
        }

        internal virtual void Dispose(bool disposing) {
            if (disposing) {
                try {
                    if (bl != null) {
                        bl.Stop();
                        bl = null;
                        serverStarted = false;
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task ReceiveConnection() {
            try {
                if (!serverStarted) {
                    bl = new BluetoothListener(LOCAL_MAC, BluetoothService.SerialPort);
                    bl.Start(1);
                    serverStarted = true;
                    Task<BluetoothClientWrapper> task = Task<BluetoothClientWrapper>.Factory.FromAsync(bl.BeginAcceptBluetoothClient, AcceptConnection, bl);
                    BluetoothClientWrapper result = await task;
                    if (task.IsCompleted) {
                        BluetoothRemoteClient = result;
                        //BluetoothRemoteClient.Listener = bl;
                    }
                }
            }
            catch (Exception ex) {
                serverStarted = false;
                Console.WriteLine(ex);
            }
        }

        private Dictionary<string, string> JsonParser(string code) {
            return (Dictionary<string, string>)JsonConvert.DeserializeObject(code);
        }

        public void VerifyClient() {
                BluetoothRemoteClient.BluetoothDeviceInfo.Refresh();
                if (BluetoothRemoteClient.BluetoothDeviceInfo.Authenticated && 
                    BluetoothRemoteClient.BluetoothDeviceInfo.Remembered && 
                    BluetoothRemoteClient.BluetoothDeviceInfo.Connected) {
                        return;
                }
                throw new Exception("Not Authenticated!");
        }
        BluetoothClientWrapper AcceptConnection(IAsyncResult result) {
            if (result.IsCompleted) {
                try {
                    BluetoothClient bc = ((BluetoothListener)result.AsyncState).EndAcceptBluetoothClient(result);
                    if (!bc.Authenticate) Pair(bc.RemoteEndPoint.Address, (new Random()).Next(6,6).ToString());
                    return new BluetoothClientWrapper(bc);
                } catch (Exception ex) {
                    return null;
                }
            }
            throw new Exception("Callback not completed!");
        }

        public BluetoothClientWrapper Connect(BluetoothDeviceInfo device, string DEVICE_PIN) {
            // check if device is paired
            if (device.Authenticated) {
                bool connectSuccess = false;
                // set pin of device to connect with
                localClient.SetPin(DEVICE_PIN);
                // async connection method
                //localClient.BeginConnect(device.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), device);

                IAsyncResult ar = localClient.BeginConnect(localClient.RemoteEndPoint as BluetoothEndPoint, null, device);

                WaitHandle connectionWait = ar.AsyncWaitHandle;
                try {
                    if (!ar.AsyncWaitHandle.WaitOne(5000, false)) {
                        localClient.Close();
                        connectSuccess = false;
                    }

                    localClient.EndConnect(ar);
                }
                finally {
                    connectionWait.Close();
                }

                if (!connectSuccess) throw new Exception("Timeout waiting for remoteEndPoint to accept bluetooth connection.");

                return new BluetoothClientWrapper(localClient);
                //return Task<BluetoothClientWrapper>.Factory.FromAsync(localClient.BeginConnect, Connect, device.DeviceAddress, BluetoothService.SerialPort, device);
            }
            throw new Exception("Not Authenticated!");
        }

        // callback
        BluetoothClientWrapper Connect(IAsyncResult ar) {
            if (ar.IsCompleted) {
                BluetoothClient bluetoothClient = (BluetoothClient)ar.AsyncState;
                bluetoothClient.EndConnect(ar);

                return new BluetoothClientWrapper(bluetoothClient);
            }
            throw new Exception();
        }
    }
}
