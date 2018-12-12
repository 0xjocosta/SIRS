using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HostLocker {
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : UserControl, ISwitchable {
        BackgroundWorker bg;
        BluetoothAddress _selectedDeviceAddress;
        BluetoothManager bm;
        private UserDevice UserDevice { get; set; }

        public RegisterWindow() {
            InitializeComponent();
            bg = new BackgroundWorker();
            bm = new BluetoothManager();
            bg.DoWork += bg_DoWork;
            bg.RunWorkerCompleted += bg_RunWorkerCompleted;
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            device_list.ItemsSource = (List<Device>)e.Result;
            pb.Visibility = Visibility.Hidden;
        }

        void bg_DoWork(object sender, DoWorkEventArgs e) {
            /*if(BluetoothRadio.IsSupported) {
                SelectBluetoothDeviceDialog dialog = new SelectBluetoothDeviceDialog();
                dialog.ShowDialog();
            } else {*/

            BluetoothClient bc = new BluetoothClient();
            BluetoothDeviceInfo[] devices = bc.DiscoverDevices(255, true, true, true, false);
            bm.deviceList = devices.OfType<BluetoothDeviceInfo>().ToList();
            List<Device> devicesWrapper = new List<Device>();
            for (int i = 0; i < devices.Length; i++) {
                Device device = new Device(devices[i]);
                devicesWrapper.Add(device);
            }
            e.Result = devicesWrapper;
        }

        private void btn_find_Click(object sender, RoutedEventArgs e) {
            if (!bg.IsBusy) {
                pb.Visibility = Visibility.Visible;
                bg.RunWorkerAsync();
            }
        }

        private void device_list_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (device_list.SelectedItem != null) {
                Device device = (Device)device_list.SelectedItem;
                connect_to_device.Visibility = Visibility.Visible;
                _selectedDeviceAddress = device.DeviceAddress;
            } else {
                connect_to_device.Visibility = Visibility.Hidden;
                _selectedDeviceAddress = null;
            }
        }

        private void pair_to_device_Click(object sender, RoutedEventArgs e) {
            if (_selectedDeviceAddress != null) {
                bm.Pair(_selectedDeviceAddress, "1111");
            }
        }

        private async void register_btn_Click(object sender, RoutedEventArgs e) {
            listen_btn.Visibility = Visibility.Hidden;
            register_btn.Visibility = Visibility.Hidden;
            //SetVisibilityOfElements(new object[] {pb, stop_listen_btn, QrCodeImage}, Visibility.Visible);
            pb.Visibility = Visibility.Visible;
            stop_listen_btn.Visibility = Visibility.Visible;
            QrCodeImage.Visibility = Visibility.Visible;

            UserDevice = new UserDevice();

            QrCodeManager qrCodeManager = new QrCodeManager(UserDevice);
            QrCodeImage.Source = qrCodeManager.GenerateQrImage();
            QrCodeImage.Visibility = Visibility.Visible;

            await bm.ReceiveConnection();

            BluetoothClientWrapper bluetoothRemoteClient = bm.BluetoothRemoteClient;

            if (bluetoothRemoteClient != null) {
                bm.VerifyClient();
                UserDevice.BluetoothConnection = bluetoothRemoteClient;
                string messageReceived = bluetoothRemoteClient.ReadFromBtDevice();
                JsonRemote messageDecrypted = UserDevice.DecryptedObjReceived(messageReceived);
                UserDevice.DevicePublicKey = messageDecrypted.PublicKey.RSAParameters;
                Switcher.Switch(new FilesWindow(), UserDevice);
            }

            QrCodeImage.Source = null;
            //SetVisibilityOfElements(new object[] { pb, QrCodeImage }, Visibility.Hidden);
            pb.Visibility = Visibility.Hidden;
            QrCodeImage.Visibility = Visibility.Hidden;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            bm.Dispose(true);
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        public void SetVisibilityOfElements(object[] elements, Visibility visibility)
        {
            foreach (UserControl element in elements)
            {
                element.Visibility = visibility;
            }
        }

        private async void listen_btn_Click(object sender, RoutedEventArgs e) {
            listen_btn.Visibility = Visibility.Hidden;
            register_btn.Visibility = Visibility.Hidden;
            pb.Visibility = Visibility.Visible;
            stop_listen_btn.Visibility = Visibility.Visible;

            await bm.ReceiveConnection();

            BluetoothClientWrapper bluetoothRemoteClient = bm.BluetoothRemoteClient;

            if (bluetoothRemoteClient != null) {
                bm.VerifyClient();
                uint userSap = bluetoothRemoteClient.GetClient().RemoteEndPoint.Address.Sap;
                // Get the encrypted key from memory if the user exists.

                if (!File.Exists($"C:\\Users\\extre\\Desktop\\HostLocker\\user_{userSap}_data.dat"))
                {
                    throw new Exception("User does not exist.");
                }

                UserData user = DataProtector.LoadData(userSap);
                UserDevice = new UserDevice(user);
                UserDevice.BluetoothConnection = bluetoothRemoteClient;

                string request = UserDevice.PrepareDecryptRequest(UserDevice.EncryptedSymmetricKey);
                bluetoothRemoteClient.SendMessage(request);
                string requestResponse = bluetoothRemoteClient.ReadFromBtDevice();
                //Handle the response
                string decryptedMessage = UserDevice.DecodeAndDecryptMessage(requestResponse);
                JsonRemote decryptedObject = JsonConvert.DeserializeObject<JsonRemote>(decryptedMessage);
                UserDevice.aesManager.SetKey(JsonConvert.DeserializeObject<byte[]>(decryptedObject.DecipheredContent),
                    user.UserAesInitVect);

                Switcher.Switch(new FilesWindow(), UserDevice);
            }

            pb.Visibility = Visibility.Hidden;
            listen_btn.Visibility = Visibility.Visible;
            register_btn.Visibility = Visibility.Visible;
            stop_listen_btn.Visibility = Visibility.Hidden;
            //bm.Dispose(true);
        }

        /*
         https://ourcodeworld.com/articles/read/471/how-to-encrypt-and-decrypt-files-using-the-aes-encryption-algorithm-in-c-sharp
         *https://www.didisoft.com/net-openpgp/examples/keystore/
         * https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata?redirectedfrom=MSDN&view=netframework-4.7.2
         * https://stackoverflow.com/questions/4967325/best-way-to-store-encryption-keys-in-net-c-sharp
         *
         */
    }
}
