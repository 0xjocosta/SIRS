using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;

using InTheHand.Net.Bluetooth;
using InTheHand.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth.AttributeIds;

namespace HostLocker {

    public partial class Server{

        // Threads

        Thread AcceptAndListeningThread;

        // helper variable

        Boolean isConnected = false;

        //bluetooth stuff

        BluetoothClient btClient;  //represent the bluetooth client connection

        BluetoothListener btListener; //represent this server bluetoothdevice

        public Server() {
            
            /*InitializeComponent();*/

            //when the bluetooth is supported by this computer

            if (BluetoothRadio.IsSupported) {

                /*UpdateLogText("Bluetooth Supported!");

                UpdateLogText("—————————–");

                //getting device information

                UpdateLogText("Primary Bluetooth Radio Name:" + BluetoothRadio.PrimaryRadio.Name);

                UpdateLogText("Primary Bluetooth Radio Address: " + BluetoothRadio.PrimaryRadio.LocalAddress);

                UpdateLogText("Primary Bluetooth Radio Manufacturer: " + BluetoothRadio.PrimaryRadio.Manufacturer);

                UpdateLogText("Primary Bluetooth Radio Mode: " + BluetoothRadio.PrimaryRadio.Mode);

                UpdateLogText("Primary Bluetooth Radio Software Manufacturer: " + BluetoothRadio.PrimaryRadio.SoftwareManufacturer);

                UpdateLogText("—————————–");*/

                //creating and starting the thread
                /*
                AcceptAndListeningThread = new Thread(AcceptAndListen);

                AcceptAndListeningThread.Start();
                */
            }

            else {

                /*UpdateLogText("Bluetooth not Supported!");*/

            }

        }

        //the function of the thread

        public void Connect() {
            while (true) {

                if (isConnected) {
                    try {
                        NetworkStream stream = btClient.GetStream();

                        Byte[] bytes = new Byte[512];

                        String retrievedMsg = "";

                        stream.Read(bytes, 0, 512);

                        stream.Flush();

                        for (int i = 0; i < bytes.Length; i++) {

                            retrievedMsg += Convert.ToChar(bytes[i]);

                        }

                        if (!retrievedMsg.Contains("server check")) {
                            sendMessage("Message Received!");
                        }
                    }

                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                        isConnected = btClient.Connected;
                    }
                }

                else {
                    try {
                        btListener = new BluetoothListener(BluetoothService.TcpProtocol);
                        btListener.Start();
                        btClient = btListener.AcceptBluetoothClient();
                        isConnected = btClient.Connected;
                    }

                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        public void AcceptAndListen() {

            while (true) {

                if (isConnected) {

                    //TODO: if there is a device connected

                    //listening

                    try {

                        /*UpdateLogTextFromThread("Listening….");*/

                        NetworkStream stream = btClient.GetStream();

                        Byte[] bytes = new Byte[512];

                        String retrievedMsg = "";

                        stream.Read(bytes, 0, 512);

                        stream.Flush();

                        for (int i = 0; i < bytes.Length; i++) {

                            retrievedMsg += Convert.ToChar(bytes[i]);

                        }

                        /*UpdateLogTextFromThread(btClient.RemoteMachineName + " : " +retrievedMsg);

                        UpdateLogTextFromThread("");*/

                        if (!retrievedMsg.Contains("server check")) {

                            sendMessage("Message Received!");

                        }

                    }

                    catch (Exception ex) {

                        /*UpdateLogTextFromThread("There is an error while listening connection");

                        UpdateLogTextFromThread(ex.Message);*/

                        isConnected = btClient.Connected;

                    }

                }

                else {

                    //TODO: if there is no connection

                    // accepting

                    try {

                        btListener = new BluetoothListener(BluetoothService.TcpProtocol);
                        /*
                        UpdateLogTextFromThread("Listener created with TCP Protocol service " +BluetoothService.TcpProtocol);

                        UpdateLogTextFromThread("Starting Listener….");*/

                        btListener.Start();
                        /*
                        UpdateLogTextFromThread("Listener Started!");

                        UpdateLogTextFromThread("Accepting incoming connection….");*/

                        btClient = btListener.AcceptBluetoothClient();

                        isConnected = btClient.Connected;

                        //UpdateLogTextFromThread("A Bluetooth Device Connected!");

                    }

                    catch (Exception e) {
                        /*
                        UpdateLogTextFromThread("There is an error while accepting connection");

                        UpdateLogTextFromThread(e.Message);

                        UpdateLogTextFromThread("Retrying….");
                        */

                    }

                }

            }

        }

        //this section is to create a method that allow thread accessing form’s component
    
        //we can’t update the text of the textbox directly from thread, so, we use this delegate function
        /*
        delegate void UpdateLogTextFromThreadDelegate(String msg);

        public void UpdateLogTextFromThread(String msg) {

            if (!this.IsDisposed && logsText.InvokeRequired) {

                logsText.Invoke(new
                UpdateLogTextFromThreadDelegate(UpdateLogText), new Object[] { msg });

            }

        }*/

        //just ordinary function to update the log text.

        //after updating, we move the cursor to the end of text and scroll it to the cursor.
    
        /*
        public void UpdateLogText(String msg) {

            logsText.Text += msg + Environment.NewLine;

            logsText.SelectionStart = logsText.Text.Length;

            logsText.ScrollToCaret();

        }*/

        //function to send message to the client

        public Boolean sendMessage(String msg) {

            try {

                if (!msg.Equals("")) {

                    UTF8Encoding encoder = new UTF8Encoding();

                    NetworkStream stream = btClient.GetStream();

                    stream.Write(encoder.GetBytes(msg + "\n"), 0, encoder.GetBytes(msg).Length);

                    stream.Flush();

                }
            }

            catch (Exception ex) {

                /*UpdateLogTextFromThread("There is an error while sending message");

                UpdateLogTextFromThread(ex.Message);*/

                try {
                    isConnected = btClient.Connected;
                    btClient.GetStream().Close();
                    btClient.Dispose();
                    btListener.Server.Dispose();
                    btListener.Stop();
                }

                catch (Exception) {

                }

                return false;

            }

            return true;

        }

        //when closing or exiting application, we have to close connection and aborting the thread.
    
        //Otherwise, the process of the thread still running in the background.

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {

            try {

                AcceptAndListeningThread.Abort();

                btClient.GetStream().Close();

                btClient.Dispose();

                btListener.Stop();

            }

            catch (Exception) {

            }

        }
        /*
        private void sendBtn_Click(object sender, EventArgs e) {
            sendMessage(messageText.Text);
        }*/
    }

}