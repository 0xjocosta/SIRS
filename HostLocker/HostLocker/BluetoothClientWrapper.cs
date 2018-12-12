using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HostLocker {
    class BluetoothClientWrapper {
        private BluetoothClient _bluetoothClient;
        public BluetoothDeviceInfo BluetoothDeviceInfo { get; set; }
        //public BluetoothListener Listener { get; set; }

        public BluetoothClientWrapper(BluetoothClient bc) {
            _bluetoothClient = bc;
            BluetoothDeviceInfo = new BluetoothDeviceInfo(bc.RemoteEndPoint.Address);
        }

        public BluetoothClient GetClient() {
            return _bluetoothClient;
        }

        public bool SendMessage(String msg) {
            try {
                if (msg.Trim().Length != 0) {
                    Console.WriteLine(msg);
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    NetworkStream stream = _bluetoothClient.GetStream();

                    stream.Write(encoder.GetBytes(msg + "\n"), 0, encoder.GetBytes(msg).Length);
                    stream.Flush();
                }
            }

            catch (Exception ex) {
                try {
                    Console.WriteLine("Sent: " + ex);
                    _bluetoothClient.GetStream().Close();
                    _bluetoothClient.Dispose();
                }
                catch (Exception e) {
                    Console.WriteLine("Ex2: " + e);
                }
                return false;
            }
            return true;
        }

        public string ReadFromBtDevice()
        {
            string response ="";
            try {
                NetworkStream stream = _bluetoothClient.GetStream();
                byte[] data = new byte[1024];
                using (MemoryStream ms = new MemoryStream()) {
                    int numBytesRead;
                    while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0) {
                        ms.Write(data, 0, numBytesRead);
                        response = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
                        if(!stream.DataAvailable) break;
                    }

                    return response;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
