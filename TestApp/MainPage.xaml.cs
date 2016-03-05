// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestAppUWP
{
    using Radios.RF24;
    using System.Diagnostics;
    using System.Text;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const byte CS_LINE = 0;
        private const byte CE_PIN = 22;
        private const byte IRQ_PIN = 4;
        private const byte CHANNEL = 10;
        private byte[] MY_ADDRESS = Encoding.UTF8.GetBytes("MYPI2");
        private byte[] SEND_ADDRESS = Encoding.UTF8.GetBytes("MYPI1");

        private string[] MESSAGES = {"Message1", "TestMessage", "Another message", "this one is a really really really really long message compared to the others"};

        RF24 radio;
        bool isInitialized = false;

        public MainPage()
        {
            this.InitializeComponent();
            ButtonSend.Click += ButtonSend_Click;

            Debug.WriteLine("THIS IS A TEST COMMENT");
            radio = new RF24();

            radio.OnDataReceived += Radio_OnDataReceived;
            radio.OnTransmitFailed += Radio_OnTransmitFailed;
            radio.OnTransmitSuccess += Radio_OnTransmitSuccess;

            radio.Initialize(CE_PIN, CS_LINE, IRQ_PIN);
            radio.Address = MY_ADDRESS;
            radio.Channel = CHANNEL;
            
            radio.IsEnabled = true;
            var myAddress = radio.Address;
            Debug.WriteLine("I am " + new string(Encoding.UTF8.GetChars(myAddress)));

            Debug.WriteLine("PA: " + radio.PowerLevel);
            Debug.WriteLine("IsAutoAcknowledge: " + radio.IsAutoAcknowledge);
            Debug.WriteLine("Channel: " + radio.Channel);
            Debug.WriteLine("DataRate: " + radio.DataRate);
            Debug.WriteLine("IsDynamicAcknowledge: " + radio.IsDyanmicAcknowledge);
            Debug.WriteLine("IsDynamicPayload: " + radio.IsDynamicPayload);
            Debug.WriteLine("IsEnabled: " + radio.IsEnabled);
            Debug.WriteLine("Frequency: " + radio.Frequency);
            Debug.WriteLine("IsInitialized: " + radio.IsInitialized);
            Debug.WriteLine("IsPowered: " + radio.IsPowered);

            isInitialized = true;
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            //byte[] toSend = new byte[MessageSend.Text.Length + 1];
            //Array.Copy(Encoding.UTF8.GetBytes(MessageSend.Text), toSend, MessageSend.Text.Length);
            radio.SendTo(SEND_ADDRESS, Encoding.UTF8.GetBytes(MessageSend.Text));
        }

        private void Radio_OnTransmitSuccess()
        {
            Debug.WriteLine("Transmit Succeeded!");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => MessageStatus.Text = "Transmit Succeeded");
        }

        private void Radio_OnTransmitFailed()
        {
            Debug.WriteLine("Transmit FAILED");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => MessageStatus.Text = "Transmit FAILED");
        }

        private void Radio_OnDataReceived(byte[] data)
        {
            Debug.WriteLine("Received: " + Encoding.UTF8.GetString(data));
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => MessageReceive.Text = Encoding.UTF8.GetString(data));
        }
    }
}
