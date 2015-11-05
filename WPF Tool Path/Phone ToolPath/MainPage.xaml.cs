using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

using NetworkIt;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Phone_ToolPath
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Client client;
        string username = "some_user_name"; // use your name or nickname here
        string address = "581.cpsc.ucalgary.ca";
        int port = 8000;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            client = new Client(username, address, port);

            client.Connected += Client_Connected;
            client.Error += Client_Error;
            client.MessageReceived += Client_MessageReceived;
        }

        async private void Client_MessageReceived(object sender, NetworkItMessageEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Debug.WriteLine("Client_MessageReceived: " + e.ReceivedMessage.Name);
                if (e.ReceivedMessage.Name == "RFID_TAG")
                {
                    TagLabel.Text = e.ReceivedMessage.GetField("rfidTag");
                } else if (e.ReceivedMessage.Name == "SERVO_POSITION")
                {

                }// Update your UI objects here (e.g. sliders, ellipses, etc.)    
            });
        }

        private void Client_Error(object sender, EventArgs e)
        {
            throw new Exception();
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("Connected to server");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
