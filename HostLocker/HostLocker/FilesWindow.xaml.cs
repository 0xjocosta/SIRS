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

namespace HostLocker {
    /// <summary>
    /// Interaction logic for FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : UserControl, ISwitchable {
        private AesManager aes;
        private UserDevice Device;

        public FilesWindow() {
            InitializeComponent();
            aes = new AesManager();
        }

        private void add_content_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            //openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; File Filter
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                    lbFiles.Items.Add(Path.GetFullPath(filename));
            }
        }

        public void UtilizeState(object state) {
            if (state != null)
            {
                Device = (UserDevice) state;
            }
        }

        private void remove_content_Click(object sender, RoutedEventArgs e)
        {
            var selected = lbFiles.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected) lbFiles.Items.Remove(item);
        }

        private void save_content_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in lbFiles.Items)
            {
                Device.FilesList.Add(file.ToString());
                aes.EncryptFile_Aes(file.ToString());
            }

            DataProtector.SaveData(CreateDataObject());
        }

        private void back_btn_Click(object sender, RoutedEventArgs e) {
            foreach (var file in lbFiles.Items) {
                //Device.FilesList.Remove(file.ToString());
                aes.DecryptFile_Aes(file.ToString()+".aes", file.ToString() + ".dec");
            }
        }

        private UserData CreateDataObject()
        {
            UserData ud = new UserData();
            //ud.BlDeviceInfo = Device.BlDeviceInfo;
            ud.DevicePublicKey = Device.DevicePublicKey;
            //ud.EncryptedUserAesKey = RSAManager.Encrypt(Encoding.ASCII.GetString(Device.SymmetricKey.Key), Device.DevicePublicKey);
            ud.UserAesInitVect = Device.SymmetricKey.InitVect;
            ud.UserPrivKey = Device.RSAKeys.PrivKey;
            ud.UserPubKey = Device.RSAKeys.PubKey;
            ud.UserSecretKey = Device.DigestKey.SecretKey;
            ud.files = Device.FilesList;

            return ud;
        }
    }
}
