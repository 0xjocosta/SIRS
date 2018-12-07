using System;
using System.Collections;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Key
{
    [Activity(Label = "KeyAndroid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { button.Text = $"{count++} clicks!"; };

            SecurityManager securityManager = new SecurityManager();
            Console.WriteLine("USING GET PUBLIC KEY");
            //Console.WriteLine(securityManager.GetPublicKey());

            //#################################################################
            // My Code

            //Connection Button
            Button connectionButton = FindViewById<Button>(Resource.Id.connectHost);

            connectionButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(ConnectionActivity));
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

