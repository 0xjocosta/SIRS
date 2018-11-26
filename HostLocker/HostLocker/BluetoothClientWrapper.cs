﻿using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HostLocker {
    class BluetoothClientWrapper {
        private BluetoothClient _bluetoothClient;

        public BluetoothClientWrapper(BluetoothClient bc) {
            _bluetoothClient = bc;
        }

        public bool sendMessage(String msg) {
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

        public string ReadFromBtDevice() {
            try {
                NetworkStream stream = _bluetoothClient.GetStream();
                byte[] bytes = new byte[1024];
                string retrievedMsg = "";

                stream.Read(bytes, 0, 512);
                stream.Flush();

                for (int i = 0; i < bytes.Length; i++) {
                    retrievedMsg += Convert.ToChar(bytes[i]);
                }

                return retrievedMsg;
            }

            catch (Exception ex) {
                return "read ex " + ex.Message;
            }
        }
    }
}
