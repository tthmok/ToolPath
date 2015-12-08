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

using Windows.UI.Xaml.Shapes;
using System.Diagnostics;
using Windows.UI.ViewManagement;
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
        string username = "totally_a_name"; // use your name or nickname here
        string address = "581.cpsc.ucalgary.ca";
        int port = 8000;

        Ellipse touchPoint;

        public MainPage()
        {
            Debug.WriteLine("Phone MainPage");
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            client = new Client(username, address, port);

            client.Connected += Client_Connected;
            client.Error += Client_Error;
            client.MessageReceived += Client_MessageReceived;

            MeasureButton.Click += MeasureButton_Click;
            MeasureButton_Hx.Click += MeasureButton_Hx_Click;
            GoToHxButton.Click += GoToHxButton_Click;

            // manual rfid button
            ScanText.PointerReleased += ScanText_PointerReleased;

            touchPoint = new Ellipse()
            {
                Width = 25,
                Height = 25,
                Fill = new SolidColorBrush(Windows.UI.Colors.Red),
                RenderTransform = new CompositeTransform()
            };
            touchPoint.Visibility = Visibility.Collapsed;

            Container.Children.Add(touchPoint);
            Container.PointerReleased += Container_PointerReleased;

            Debug.WriteLine("Phone MainPage End");
        }

        void ScanText_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ScanText.PointerReleased -= ScanText_PointerReleased;

            LaunchScreen.Visibility = Visibility.Collapsed;
            MeasureScreen.Visibility = Visibility.Collapsed;

            RFIDScreen.Visibility = Visibility.Visible;
            TagLabel.Text = "018948ngio93h";
        }

        private void GoToHxButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchScreen.Visibility = Visibility.Collapsed;
            RFIDScreen.Visibility = Visibility.Collapsed;
            MeasureScreen.Visibility = Visibility.Collapsed;
            HxScreen.Visibility = Visibility.Visible;
            touchPoint.Visibility = Visibility.Collapsed;
        }

        private void MeasureButton_Hx_Click(object sender, RoutedEventArgs e)
        {
            LaunchScreen.Visibility = Visibility.Collapsed;
            RFIDScreen.Visibility = Visibility.Collapsed;
            HxScreen.Visibility = Visibility.Collapsed;

            MeasureScreen.Visibility = Visibility.Visible;
            touchPoint.Visibility = Visibility.Visible;
        }

        private void Container_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (touchPoint.RenderTransform as CompositeTransform).TranslateX = e.GetCurrentPoint(Container).Position.X - touchPoint.Width / 2;
            (touchPoint.RenderTransform as CompositeTransform).TranslateY = e.GetCurrentPoint(Container).Position.Y - touchPoint.Height / 2;
        }
        

        private void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchScreen.Visibility = Visibility.Collapsed;
            RFIDScreen.Visibility = Visibility.Collapsed;
            HxScreen.Visibility = Visibility.Collapsed;

            MeasureScreen.Visibility = Visibility.Visible;
            touchPoint.Visibility = Visibility.Visible;
        }

        async private void Client_MessageReceived(object sender, NetworkItMessageEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (e.ReceivedMessage.Name == "RFID_TAG")
                {
                    LaunchScreen.Visibility = Visibility.Collapsed;
                    MeasureScreen.Visibility = Visibility.Collapsed;

                    RFIDScreen.Visibility = Visibility.Visible;

                    TagLabel.Text = e.ReceivedMessage.GetField("rfidTag");
                } else if (e.ReceivedMessage.Name == "SERVO_POSITION")
                {
                    double servoPosition = Double.Parse(e.ReceivedMessage.GetField("position"));
                    double servoPositionMin = Double.Parse(e.ReceivedMessage.GetField("positionMin"));
                    double servoPositionMax = Double.Parse(e.ReceivedMessage.GetField("positionMax"));

                    // hack caliper measure between 0-60mm
                    CaliperMeasure.Text = string.Format("{0:0.0}", CosineInterpolate(60.0, 0.0, (servoPosition/(servoPositionMax-servoPositionMin)))) + "mm";
                    Debug.WriteLine("CaliperMeasure.Text: " + CaliperMeasure.Text);
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

        // based on http://paulbourke.net/miscellaneous/interpolation/
        static public double CosineInterpolate(double y1, double y2, double mu)
        {
            double mu2;

            mu2 = (1 - Math.Cos(mu * Math.PI)) / 2.0f;
            return (y1 * (1 - mu2) + y2 * mu2);
        }
    }
}
