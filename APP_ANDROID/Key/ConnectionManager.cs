using Android.Bluetooth;
using Android.Widget;
using Java.Util;
using System;
using System.IO;
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

        public ConnectionManager()
        {
            private void ConnectingToDevice(BluetoothDevice dev)
            {
                UUID myUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                BluetoothSocket tmp = null;
                try
                {
                    // MY_UUID is the app's UUID string, also used by the server code
                    tmp = dev.CreateRfcommSocketToServiceRecord(myUUID);
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine("FAILED TO CREATE SOCKET");
                }
                mmSocket = tmp;
                BluetoothAdapter defaultAdapter = BluetoothAdapter.DefaultAdapter;
                defaultAdapter.CancelDiscovery();
            }
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

        //security manager here 
            //SendPubKey();
        //chamada da funcao com activityUI portanto duvidum que fique
            //ListeningFromSocketAsync();

        }
    }
}
