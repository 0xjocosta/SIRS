using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Shapes;
using QRCoder;

namespace HostLocker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class QRCodeWindow : Window
    {

        public QRCodeWindow(RoutedEventArgs Image)
        {
            InitializeComponent();
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            //Image.Graphics.DrawImage(qrCodeImage, new System.Drawing.Point(100,100));

        }
    }
}
