using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            BluetoothDeviceInfo[] devices = bc.DiscoverDevices();
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
                txt_authenticated.Text = device.Authenticated.ToString();
                txt_connected.Text = device.Connected.ToString();
                txt_devicename.Text = device.DeviceName;
                txt_lastseen.Text = device.LastSeen.ToString();
                txt_lastused.Text = device.LastUsed.ToString();
                txt_nap.Text = device.Nap.ToString();
                txt_remembered.Text = device.Remembered.ToString();
                txt_sap.Text = device.Sap.ToString();
            } else {
                connect_to_device.Visibility = Visibility.Hidden;
                selectedDeviceAddress = null;
            }
        }

        private async void connect_to_device_Click(object sender, RoutedEventArgs e) {
            if (selectedDeviceAddress != null) {
                bm.Pair(selectedDeviceAddress, "1111");
                BluetoothClientWrapper connection = bm.Connect(bm.GetDevicesByAddress(selectedDeviceAddress), "1111");
                connection.sendMessage("HELLO");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            Console.WriteLine("Started");
            BluetoothClientWrapper bc = await bm.ReceiveConnection();
            Console.WriteLine(bc.ReadFromBtDevice());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            try {
                bm.bl.Stop();
                Console.WriteLine("Stop");
            }
            catch (Exception) { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            QRCodeWindow qrcode = new QRCodeWindow(e);
            qrcode.Show();
            Console.WriteLine("asdasdasdasd");
        }
    }
}
