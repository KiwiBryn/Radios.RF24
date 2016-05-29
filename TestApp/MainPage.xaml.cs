// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    using Radios.RF24;
    using System;
    using System.Diagnostics;
    using System.Text;
    using Windows.Foundation;
    using Windows.System.Threading;
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
        private byte[] MY_ADDRESS = Encoding.UTF8.GetBytes("HUB01");

        public RF24 Radio;
        bool isInitialized = false;
        DispatcherTimer timer = new DispatcherTimer();
        private IAsyncAction pollingThread = null;     

        public MainPage()
        {
            this.InitializeComponent();
            SendButton.Click += ButtonSend_Click;
            EnableInterrupt.Checked += EnableInterrupt_Checked;
            EnableInterrupt.Unchecked += EnableInterrupt_Unchecked;
            
            Radio = new RF24();

            Radio.OnDataReceived += Radio_OnDataReceived;
            Radio.OnTransmitFailed += Radio_OnTransmitFailed;
            Radio.OnTransmitSuccess += Radio_OnTransmitSuccess;

            Radio.Initialize(CE_PIN, CS_LINE, IRQ_PIN);
            Radio.Address = MY_ADDRESS;
            Radio.Channel = CHANNEL;
            
            Radio.IsEnabled = true;

            PowerLevel.ItemsSource = Enum.GetValues(typeof(PowerLevel));
            DataRate.ItemsSource = Enum.GetValues(typeof(DataRate));
            DataContext = Radio;

            timer.Interval = TimeSpan.FromMilliseconds(5);
            timer.Tick += Timer_Tick;
            timer.Stop();

            Debug.WriteLine("Address: " + Encoding.UTF8.GetString(Radio.Address));
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

        private void Timer_Tick(object sender, object e)
        {
            
        }

        private void EnableInterrupt_Checked(object sender, RoutedEventArgs e)
        {
            if((bool)EnableInterrupt.IsChecked)
            {
                //timer.Stop();
                pollingThread.Cancel();
                Radio.InterruptPin = IRQ_PIN;                
            }
        }

        private void EnableInterrupt_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!((bool)EnableInterrupt.IsChecked))
            {
                Radio.InterruptPin = null;
                //timer.Start();
                
                pollingThread = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) => {
                    while (true)
                    {
                        if (Radio.IsDataAvailable)
                        {
                            Debug.WriteLine("Trying to poll for read...");
                            byte[] payload = Radio.Read();
                            if (payload != null)
                            {
                                Debug.WriteLine("Received (Polling): " + Encoding.UTF8.GetString(payload));
                                if (isInitialized)
                                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReceiveBuffer.Text = Encoding.UTF8.GetString(payload));
                            }                            
                        }
                    }
                }), WorkItemPriority.High, WorkItemOptions.TimeSliced);
            }
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)EnableInterrupt.IsChecked)
            {
                Radio.SendTo(Encoding.UTF8.GetBytes(SendToAddress.Text), Encoding.UTF8.GetBytes(SendBuffer.Text));
            }
            else
            {
                bool sent = Radio.SendTo(Encoding.UTF8.GetBytes(SendToAddress.Text), Encoding.UTF8.GetBytes(SendBuffer.Text));
                if (sent)
                {
                    Debug.WriteLine("Transmit Succeeded! (Polling)");
                    if (isInitialized)
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit Succeeded (Polling)");
                }
                else
                {
                    Debug.WriteLine("Transmit FAILED (Polling)");
                    if (isInitialized)
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit FAILED (Polling)");
                }
            }
        }

        private void Radio_OnTransmitSuccess()
        {
            Debug.WriteLine("Transmit Succeeded! (Interrupt)");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit Succeeded (Interrupt)");
        }

        private void Radio_OnTransmitFailed()
        {
            Debug.WriteLine("Transmit FAILED (Interrupt)");
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SendStatus.Text = "Transmit FAILED (Interrupt)");
        }

        private void Radio_OnDataReceived(byte[] data)
        {
            Debug.WriteLine("Received (Interrupt): " + Encoding.UTF8.GetString(data));
            if (isInitialized)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReceiveBuffer.Text = Encoding.UTF8.GetString(data));
        }
    }
}
