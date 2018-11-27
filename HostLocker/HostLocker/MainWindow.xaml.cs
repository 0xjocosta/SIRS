using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;


namespace HostLocker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        BackgroundWorker bg;
        BluetoothAddress selectedDeviceAddress;
        BluetoothManager bm;

        public MainWindow() {
            InitializeComponent();
            bg = new BackgroundWorker();
            bm = new BluetoothManager();
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
            BluetoothDeviceInfo[] devices = bc.DiscoverDevices(255, false, false, false, false);
            bm.Scan(sender, new DiscoverDevicesEventArgs(devices, sender));
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
                selectedDeviceAddress = device.DeviceAddress;
                UpdateInfo(device);
            } else {
                connect_to_device.Visibility = Visibility.Hidden;
                selectedDeviceAddress = null;
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
            JsonParser("{\"Nonce\": " + GenerateNonce() + "}");
            /*
            if (selectedDeviceAddress != null) {
                bm.Pair(selectedDeviceAddress, "1111");
            }*/
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            pb.Visibility = Visibility.Visible;
            listen_btn.Visibility = Visibility.Hidden;
            stop_listen_btn.Visibility = Visibility.Visible;
            QrCodeImage.Visibility = Visibility.Visible;
            QRCodeImage();
            await bm.ReceiveConnection();
            if (bm.RemoteClient() != null) {
                bm.VerifyClient(bm.RemoteClient().ReadFromBtDevice());
                UpdateInfo(new Device(bm.RemoteClient().GetDeviceInfo()));
                success_txt.Visibility = Visibility.Visible;
            }
            QrCodeImage.Visibility = Visibility.Hidden;
            QrCodeImage.Source = null;
            pb.Visibility = Visibility.Hidden;
            //Console.WriteLine(bc.ReadFromBtDevice());
            //bc.sendMessage("It wokrs||!!");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            bm.Dispose(true);
            stop_listen_btn.Visibility = Visibility.Hidden;
            listen_btn.Visibility = Visibility.Visible;
            pb.Visibility = Visibility.Hidden;
        }

        private ImageSource ImageSourceForBitmap(Bitmap bmp) {
            var handle = bmp.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { }
        }

        private void QRCodeImage() {
            InitializeComponent();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("{\"Nonce\": "+GenerateNonce()+ "}", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            QrCodeImage.Source = ImageSourceForBitmap(qrCodeImage);
        }

        public string GenerateNonce() {
            bm.nonce = Guid.NewGuid().ToString("N");
            return bm.nonce;
        }


        private void JsonParser(string code) {
            Dictionary<string, string> dicKeys = (Dictionary<string, string>) JsonConvert.DeserializeObject(code);
            Console.WriteLine(dicKeys["Nonce"]);
        }
    }
}
