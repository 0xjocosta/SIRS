﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using QRCoder;

namespace HostLocker {

    class QrCodeManager
    {
        private ImageSource QrImage { get; set; }

        internal UserDevice UserDevice { get; set; }

        public QrCodeManager(UserDevice ud)
        {
            UserDevice = ud;
            GenerateQrImage();
        }

        public ImageSource GenerateQrImage() {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(QrCodeContent(), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(50);

            QrImage = ImageSourceForBitmap(qrCodeImage);

            return QrImage;
        }

        private ImageSource ImageSourceForBitmap(Bitmap bmp) {
            var handle = bmp.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { }
        }

        private string QrCodeContent() {
            return JsonConvert.SerializeObject(
                new QrCodeObject(
                    UserDevice.Nonce, 
                    UserDevice.RSAKeys.GetPublicKeyString(),
                    UserDevice.DigestKey.GetSecretKeyString())
                );
        }
    }

    public class QrCodeObject {
        public string Nonce;
        public string KcPub;
        public string Kd;

        public QrCodeObject(string nonce, string kcpub, string kd) {
            Nonce = nonce;
            KcPub = kcpub;
            Kd = kd;
        }
    }
}
