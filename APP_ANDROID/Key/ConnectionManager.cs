using Android.Bluetooth;
using Android.Widget;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Key
{
    public class ConnectionManager
    {
        bool CONNECTED = false;
        BluetoothSocket mmSocket;
        Stream inputStream;
        Stream outputStream;
        TextView txtDebug;

        BluetoothDevice bluetoothDevice;
        SecurityManager securityManager;

        public ConnectionManager(BluetoothDevice dev, TextView debugger=null)
        {
            bluetoothDevice = dev;
            //Debug
            txtDebug = debugger;
        }

        public void ConnectingToDevice()
        {
            UUID myUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            BluetoothSocket tmp = null;
            try
            {
                // MY_UUID is the app's UUID string, also used by the server code
                tmp = bluetoothDevice.CreateRfcommSocketToServiceRecord(myUUID);
            }
            catch (System.IO.IOException)
            {
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
                catch (IOException)
                {
                    Console.WriteLine("FAILED TO CLOSE");
                }
                return;
            }

            // Do work to manage the connection (in a separate thread)
            ConfigConnectedSocket();
        }

        private void ConfigConnectedSocket()
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
        }

        /** Will cancel an in-progress connection, and close the socket */
        public void CancelConnection()
        {
            if (CONNECTED == false)
            {
                return;
            }
            try
            {
                mmSocket.Close();
            }
            catch (Exception exc) {
                Console.WriteLine(exc);

            }
        }

        public void SetConnectionWithInfo(string qrCodeInfo)
        {
            Console.Write("WITH INFO FROM REGISTER");
            QrCodeObject qrCodeObj = JsonConvert.DeserializeObject<QrCodeObject>(qrCodeInfo);

            securityManager = new SecurityManager(qrCodeObj.KcPub, qrCodeObj.Kd);
            //dict[nonce] = dicKeys[nonce].Value;

            ConnectingToDevice();
            SendRegisterInfo(qrCodeObj);
            Console.Write("END");
            ListeningFromSocketAsync();
        }

        public void SetConnection()
        {
            Console.Write("WITHOUT INFO");
            securityManager = new SecurityManager();
            ConnectingToDevice();
            ListeningFromSocketAsync();
        }

        private void SendRegisterInfo(QrCodeObject qrCodeObj)
        {
            securityManager.Nonce = qrCodeObj.Nonce;
            JsonRemote message = new JsonRemote();
            message.PublicKey = securityManager.GetPublicKey();
            string content = JsonConvert.SerializeObject(message);
            string strToSend = securityManager.EncryptAndEncodeMessage(content);
            WriteToSocket(strToSend);
        }

        /* Call this from the main activity to send data to the remote device */
        public void WriteToSocket(string str)
        {
            try
            {
                Console.WriteLine("WRITE TO SOCKET");
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                outputStream.Write(bytes, 0, bytes.Length);

                txtDebug.Text += "Sending: " + str + '\n';
            }
            catch (IOException) { }
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
                    await inputStream.ReadAsync(buffer, 0, 1024);
                    string str = Encoding.ASCII.GetString(buffer);
                    txtDebug.Text += "Receiving: " + str + '\n';

                    ManageConnectedSocket(str);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                    break;
                }
            }
        }

        private void ManageConnectedSocket(string buffer)
        {
            string decryptedBuffer = securityManager.DecodeAndDecryptMessage(buffer);
            JsonRemote message = JsonConvert.DeserializeObject<JsonRemote>(decryptedBuffer);

            //Send the decrypted symmetric Key
            string decryptedContent = securityManager.DecryptContentFromHost(message.ContentToDecipher);
            JsonRemote jsonMessage = new JsonRemote
            {
                DecipheredContent = decryptedContent
            };
            string content = JsonConvert.SerializeObject(jsonMessage);
            string strToSend = securityManager.EncryptAndEncodeMessage(content);
            WriteToSocket(strToSend);
        }
    }

    public class QrCodeObject
    {
        public string Nonce;
        public RSAParameters KcPub;
        public byte[] Kd;

        public QrCodeObject(string nonce, RSAParameters kcpub, byte[] kd)
        {
            Nonce = nonce;
            KcPub = kcpub;
            Kd = kd;
        }
    }
}
