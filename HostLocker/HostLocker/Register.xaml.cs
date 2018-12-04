using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace HostLocker {
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : UserControl, ISwitchable {
        BackgroundWorker bg;
        BluetoothAddress _selectedDeviceAddress;
        BluetoothManager bm;
        private UserDevice UserDevice { get; set;}

        public RegisterWindow() {
            InitializeComponent();
            bg = new BackgroundWorker();
            bm = new BluetoothManager(this);
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
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
                UpdateInfo(device);
            } else {
                connect_to_device.Visibility = Visibility.Hidden;
                _selectedDeviceAddress = null;
            }
        }

        private void UpdateInfo(Device device) {
            txt_authenticated.Text = device.Authenticated.ToString();
            txt_connected.Text = device.Connected.ToString();
            txt_devicename.Text = device.DeviceName;
            txt_lastused.Text = device.LastUsed.ToString();
            txt_remembered.Text = device.Remembered.ToString();
        }

        private async void connect_to_device_Click(object sender, RoutedEventArgs e) {
            if (_selectedDeviceAddress != null) {
                bm.Pair(_selectedDeviceAddress, "1111");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            listen_btn.Visibility = Visibility.Hidden;
            //SetVisibilityOfElements(new object[] {pb, stop_listen_btn, QrCodeImage}, Visibility.Visible);
            pb.Visibility = Visibility.Visible;
            stop_listen_btn.Visibility = Visibility.Visible;
            QrCodeImage.Visibility = Visibility.Visible;
            UserDevice = new UserDevice();
            QrCodeManager qrCodeManager = new QrCodeManager(UserDevice);
            QrCodeImage.Source = qrCodeManager.GenerateQrImage();
            await bm.ReceiveConnection();
            if (bm.BluetoothRemoteClient != null)
            {
                string devicePubKey = bm.BluetoothRemoteClient.ReadFromBtDevice();
                bm.VerifyClient();
                UserDevice.SetDeviceKey(devicePubKey);
                UserDevice.AssociateDevice(bm.BluetoothRemoteClient.BluetoothDeviceInfo);
                UpdateInfo(new Device(UserDevice.BlDeviceInfo));
                success_txt.Visibility = Visibility.Visible;
                bm.BluetoothRemoteClient.SendMessage(UserDevice.EncryptAndEncodeMessage("AKNOWLEDGE THIS NUTS NIBBA"));
                bm.Dispose(true);
            }
            QrCodeImage.Source = null;
            //SetVisibilityOfElements(new object[] { pb, QrCodeImage }, Visibility.Hidden);
            pb.Visibility = Visibility.Visible;
            QrCodeImage.Visibility = Visibility.Visible;
            //Console.WriteLine(bc.ReadFromBtDevice());
            //bc.sendMessage("It wokrs||!!");
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
    }
}
