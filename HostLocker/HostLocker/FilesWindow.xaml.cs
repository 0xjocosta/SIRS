using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using InTheHand.Net.Bluetooth;

namespace HostLocker {
    /// <summary>
    /// Interaction logic for FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : UserControl, ISwitchable {
        private UserDevice Device;
        System.Threading.Timer connTimer;

        public FilesWindow() {
            InitializeComponent();
        }

        private void add_content_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            //openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; File Filter
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    lbFiles.Items.Add(Path.GetFullPath(filename));
                    Device.FilesList.Remove(filename);
                }
            }
        }

        private void Dispose() {
            connTimer.Dispose();
        }

        private void StopTimer() {
            connTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CheckConnection(object state)
        {
            Device.BluetoothConnection.BluetoothDeviceInfo.Refresh();
            if (!Device.BluetoothConnection.BluetoothDeviceInfo.Connected)
            {
                Save();
            }
        }

        private void Save() {
            StopTimer();
            Device.InitAesManager();
            foreach (var file in lbFiles.Items) {
                Device.FilesList.Add(file.ToString());
                Device.aesManager.EncryptFile_Aes(file.ToString());
            }

            DataProtector.SaveData(CreateDataObject());
            Dispose();
            Device.BluetoothConnection.GetClient().Client.Close();
            Device.BluetoothConnection.GetClient().Dispose();

            Application.Current.Dispatcher.Invoke((Action)delegate {
                Switcher.Switch(new RegisterWindow());
            });
        }

        public void UtilizeState(object state) {
            if (state != null)
            {
                Device = (UserDevice) state;
                UpdateInfo(new Device(Device.BluetoothConnection.BluetoothDeviceInfo));
                connTimer = new Timer(CheckConnection, null, 0, 1000);
                foreach (var file in Device.FilesList)
                {
                    lbFiles.Items.Add(file);
                    Device.aesManager.DecryptFile_Aes(file + ".aes", file + ".dec");
                }
            }
        }

        private void remove_content_Click(object sender, RoutedEventArgs e)
        {
            var selected = lbFiles.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected)
            {
                lbFiles.Items.Remove(item);
                Device.FilesList.Remove(item.ToString());
            }
        }

        private void back_btn_Click(object sender, RoutedEventArgs e) {
            Save();
        }

        private void UpdateInfo(Device device) {
            txt_authenticated.Text = device.Authenticated.ToString();
            txt_connected.Text = device.Connected.ToString();
            txt_devicename.Text = device.DeviceName;
            txt_lastused.Text = device.LastUsed.ToString();
            txt_remembered.Text = device.Remembered.ToString();
        }

        private UserData CreateDataObject()
        {
            UserData ud = new UserData(new Device(Device.BluetoothConnection.BluetoothDeviceInfo));
            ud.DevicePublicKey = new RSAParametersSerializable(Device.DevicePublicKey);
            ud.EncryptedUserAesKey = Device.EncryptedSymmetricKey;
            ud.UserAesInitVect = Device.aesManager.InitVect;
            ud.UserPrivKey = new RSAParametersSerializable(Device.rsaManager.PrivKey);
            ud.UserPubKey = new RSAParametersSerializable(Device.rsaManager.PubKey);
            ud.UserSecretKey = Device.hmacManager.SecretKey;
            ud.Files = Device.FilesList;
            return ud;
        }
    }
}
