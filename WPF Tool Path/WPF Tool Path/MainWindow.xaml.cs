using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Phidgets;
using Phidgets.Events;
using NetworkItWPF;

namespace WPF_Tool_Path
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client client;
        string username = "some_user_name";
        string address = "581.cpsc.ucalgary.ca";
        int port = 8000;

        public const int KnobIndex = 0;

        public InterfaceKit interfaceKit = new InterfaceKit();

        public RFID rfidReader = new RFID();

        public Servo servoController = new Servo();
        public ServoServo servoMotor = null;

        public MainWindow()
        {
            InitializeComponent();

            client = new Client(username, address, port);

            client.Connected += Client_Connected;
            client.Error += Client_Error;
            client.MessageReceived += Client_MessageReceived;

            interfaceKit.Attach += InterfaceKit_Attach;
            interfaceKit.open();

            rfidReader.Attach += RfidReader_Attach; ;
            rfidReader.open();

            servoController.Attach += ServoController_Attach; ;
            servoController.open();
        }

        private void RfidReader_Attach(object sender, AttachEventArgs e)
        {
            rfidReader.Antenna = true;

            Console.WriteLine("RfidReader_Attach");
            // event for when a tag is found
            rfidReader.Tag += RfidReader_Tag; ;
            // event for when a tag is lost
            rfidReader.TagLost += RfidReader_TagLost; ;
        }

        private void RfidReader_TagLost(object sender, TagEventArgs e)
        {
            rfidReader.LED = false;
            Console.WriteLine("The Tag: " + e.Tag + " was lost");        }

        private void RfidReader_Tag(object sender, TagEventArgs e)
        {
            rfidReader.LED = true;
            Console.WriteLine("The Tag: " + e.Tag + " was found");


            // Create a message and populate it
            Message message = new Message("RFID_TAG");
            message.AddField<string>("rfidTag", e.Tag);

            // Send the message
            client.SendMessage(message);
        }

        private void ServoController_Attach(object sender, AttachEventArgs e)
        {
            Console.WriteLine("ServoController_Attach");

            servoMotor = servoController.servos[0];
            servoMotor.Position = 0;
            Console.WriteLine("min: " + servoMotor.PositionMin);
            Console.WriteLine("max: " + servoMotor.PositionMax);
        }

        private void InterfaceKit_Attach(object sender, AttachEventArgs e)
        {
            interfaceKit.SensorChange += InterfaceKit_SensorChange;
        }

        private void InterfaceKit_SensorChange(object sender, SensorChangeEventArgs e)
        {
            if (e.Index == KnobIndex)
            {
                Console.WriteLine("Knob change: " + e.Value);
                if (servoMotor != null)
                {
                    // normalize knob rotation
                    double normalRot = e.Value / 1000f;
                    servoMotor.Position = CosineInterpolate(servoMotor.PositionMin, servoMotor.PositionMax, normalRot);
                    Console.WriteLine("new servo position: " + servoMotor.Position);

                    // Create a message and populate it
                    Message message = new Message("SERVO_POSITION");
                    message.AddField<double>("position", servoMotor.Position);

                    // Send the message
                    client.SendMessage(message);
                }
            }
        }

        private void Client_MessageReceived(object sender, NetworkItMessageEventArgs e)
        {
            Console.WriteLine("Client_MessageReceived: " + e.ReceivedMessage.Name);
        }

        private void Client_Error(object sender, EventArgs e)
        {
            throw new Exception();
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to server");
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
