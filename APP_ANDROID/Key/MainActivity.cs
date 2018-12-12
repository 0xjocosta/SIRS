using System;
using System.Collections;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text;

namespace Key
{
    [Activity(Label = "BlueKey", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            ImageView image1View = FindViewById<ImageView>(Resource.Id.imageblue);

            //Connection Button
            Button connectionButton = FindViewById<Button>(Resource.Id.connectHost);

            connectionButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(ConnectionActivity));
                intent.PutExtra("LAST_PAGE", "MAIN");
                StartActivity(intent);
            };

            //Register Button
            Button registerButton = FindViewById<Button>(Resource.Id.register);

            registerButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RegisterActivity));
                StartActivity(intent);
            };
        }
    }
}