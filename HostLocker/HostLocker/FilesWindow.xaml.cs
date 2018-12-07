using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Security.AccessControl;

namespace HostLocker {
    /// <summary>
    /// Interaction logic for FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : UserControl, ISwitchable {
        private List<string> files;
        private AesManager aes;
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
            throw new NotImplementedException();
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
                //files.Add(file.ToString());
                aes.EncryptFile_Aes(file.ToString());
            }
        }

        private void back_btn_Click(object sender, RoutedEventArgs e) {
            foreach (var file in lbFiles.Items) {
                //files.Add(file.ToString());
                aes.DecryptFile_Aes(file.ToString()+".aes", file.ToString() + ".dec");
            }
        }
    }
}
