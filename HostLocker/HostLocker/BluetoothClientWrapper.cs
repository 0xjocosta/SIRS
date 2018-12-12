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
                    UTF8Encoding encoder = new UTF8Encoding();
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
                /*
                byte[] bytes = new byte[4096];
                string retrievedMsg = "";*/

                NetworkStream stream = _bluetoothClient.GetStream();
                byte[] data = new byte[1024];
                using (MemoryStream ms = new MemoryStream()) {
                    int numBytesRead;
                    stream.ReadTimeout = 500;
                    while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0) {
                        ms.Write(data, 0, numBytesRead);
                        response = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);

                    }

                    return response;
                }
                /*
                stream.Read(bytes, 0, 4096);
                stream.Flush();

                for (int i = 0; i < bytes.Length; i++) {
                    
                    byte c = Convert.ToByte('\0');

                    if(bytes[i] != c)
                    {
                        retrievedMsg += Convert.ToChar(bytes[i]);
                        continue;
                    }
                    break;
                }

                return retrievedMsg;*/
            }

            catch (Exception ex)
            {
                return response;
            }
        }
    }
}
