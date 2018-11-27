using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostLocker {
    class BluetoothManager {
        private bool _beginConnect;
        private BluetoothClient _bluetoothClient;
        private BluetoothClientWrapper _bluetoothRemoteClient = null;
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
        List<BluetoothDeviceInfo> deviceList = new List<BluetoothDeviceInfo>();

        public BluetoothManager() {
            LOCAL_MAC = GetBTMacAddress();
            if (LOCAL_MAC != null) {
                localEndpoint = new BluetoothEndPoint(LOCAL_MAC, BluetoothService.SerialPort);
                localClient = new BluetoothClient(localEndpoint);
                localComponent = new BluetoothComponent(localClient);
                // async methods, can be done synchronously too
                localComponent.DiscoverDevicesAsync(255, true, true, true, true, null);
                localComponent.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(Scan);
                localComponent.DiscoverDevicesComplete += new EventHandler<DiscoverDevicesEventArgs>(Scan_Complete);
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

        public BluetoothClientWrapper RemoteClient() {
            return _bluetoothRemoteClient;
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
                if (e.Devices[i].Remembered) {
                    Console.WriteLine(e.Devices[i].DeviceName + " (" + e.Devices[i].DeviceAddress + "): Device is known");
                }
                else {
                    Console.WriteLine(e.Devices[i].DeviceName + " (" + e.Devices[i].DeviceAddress + "): Device is unknown");
                }
                this.deviceList.Add(e.Devices[i]);
            }
        }

        private void Scan_Complete(object sender, DiscoverDevicesEventArgs e) {
            Console.WriteLine("Devices discovered: " + e.Devices);
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
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task ReceiveConnection() {
            try {
                if (!serverStarted) {
                    bl = new BluetoothListener(LOCAL_MAC, BluetoothService.SerialPort);
                    bl.Start(10);
                    serverStarted = true;
                    Task<BluetoothClientWrapper> task = Task<BluetoothClientWrapper>.Factory.FromAsync(bl.BeginAcceptBluetoothClient, AcceptConnection, bl);
                    BluetoothClientWrapper result = await task;
                    if (task.IsCompleted) {
                        _bluetoothRemoteClient = result;
                    }
                }
            }
            catch (Exception ex) {
                serverStarted = false;
                Console.WriteLine(ex);
            }
        }

        public void VerifyClient(string response) {
            BluetoothDeviceInfo device;
            foreach (BluetoothDeviceInfo dev in deviceList) {
                Console.WriteLine(dev.DeviceName, _bluetoothRemoteClient.GetClient().RemoteMachineName);
                if(dev.DeviceName == _bluetoothRemoteClient.GetClient().RemoteMachineName) {
                    Console.WriteLine("Auth");
                    device = dev;
                    _bluetoothRemoteClient.SetDeviceInfo(device);
                    if (device.Authenticated && device.Remembered && device.Connected) {
                        Console.WriteLine(response);
                        if (nonce != response) {
                            throw new Exception("Not the Entity Authenticated!");
                        }
                    }
                    break;
                }
            }
            throw new Exception("Not Authenticated!");
        }
        BluetoothClientWrapper AcceptConnection(IAsyncResult result) {
            if (result.IsCompleted) {
                try {
                    BluetoothClient bc = ((BluetoothListener)result.AsyncState).EndAcceptBluetoothClient(result);
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
