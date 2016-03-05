// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
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

        private string[] MESSAGES = {"Message1", "TestMessage", "Another message", "this one is a really really really really long message compared to the others"};

        public RF24 Radio;
        bool isInitialized = false;

        public MainPage()
        {
            this.InitializeComponent();
            SendButton.Click += ButtonSend_Click;

            Debug.WriteLine("THIS IS A TEST COMMENT");
            Radio = new RF24();

            Radio.OnDataReceived += Radio_OnDataReceived;
            Radio.OnTransmitFailed += Radio_OnTransmitFailed;
            Radio.OnTransmitSuccess += Radio_OnTransmitSuccess;

            Radio.Initialize(CE_PIN, CS_LINE, IRQ_PIN);
            Radio.Address = MY_ADDRESS;
            Radio.Channel = CHANNEL;
            
            Radio.IsEnabled = true;
            var myAddress = Radio.Address;
            Debug.WriteLine("I am " + new string(Encoding.UTF8.GetChars(myAddress)));

            Debug.WriteLine("PA: " + Radio.PowerLevel);
            Debug.WriteLine("IsAutoAcknowledge: " + Radio.IsAutoAcknowledge);
            Debug.WriteLine("Channel: " + Radio.Channel);
            Debug.WriteLine("DataRate: " + Radio.DataRate);
            Debug.WriteLine("IsDynamicAcknowledge: " + Radio.IsDyanmicAcknowledge);
            Debug.WriteLine("IsDynamicPayload: " + Radio.IsDynamicPayload);
            Debug.WriteLine("IsEnabled: " + Radio.IsEnabled);
            Debug.WriteLine("Frequency: " + Radio.Frequency);
            Debug.WriteLine("IsInitialized: " + Radio.IsInitialized);
            Debug.WriteLine("IsPowered: " + Radio.IsPowered);

            isInitialized = true;
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            Radio.SendTo(Encoding.UTF8.GetBytes(SendToAddress.Text), Encoding.UTF8.GetBytes(SendBuffer.Text));
        }

        private void Radio_OnTransmitSuccess()
        {
            Debug.WriteLine("Transmit Succeeded!");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit Succeeded");
        }

        private void Radio_OnTransmitFailed()
        {
            Debug.WriteLine("Transmit FAILED");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit FAILED");
        }

        private void Radio_OnDataReceived(byte[] data)
        {
            Debug.WriteLine("Received: " + Encoding.UTF8.GetString(data));
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReceiveBuffer.Text = Encoding.UTF8.GetString(data));
        }
    }
}
