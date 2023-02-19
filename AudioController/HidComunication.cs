using HidSharp;
using HidSharp.Reports;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms.VisualStyles;

namespace AudioController
{
    public class HidComunication : INotifyPropertyChanged
    {
        private const int VendorId = 0x16C0;
        private const int ProductId = 0x0486;
        private bool isConnected;

        public event PropertyChangedEventHandler PropertyChanged;

        public HidComunication(IVolumeController volumeController)
        {
            this.IsConnected = this.TryConnectDevice();
            this.StartReader();

            this.VolumeController = volumeController;
            this.VolumeController.SetDefaultRecordingEndpoint();

            DeviceList.Local.Changed += this.DeviceListChangedHandler;
        }

        private HidDevice Device { get; set; }

        public bool IsConnected
        {
            get => this.isConnected;

            set
            {
                this.isConnected = value;
                this.OnPropertyChanged();
            }
        }

        public bool RunReader { get; set; }

        private Thread Thread { get; set; }

        public IVolumeController VolumeController { get; }

        private void DeviceListChangedHandler(object sender, DeviceListChangedEventArgs e)
        {
            this.IsConnected = this.TryConnectDevice();
            this.StartReader();
        }

        private bool TryConnectDevice()
        {
            var list = DeviceList.Local;
            var hidDeviceList = list.GetHidDevices().ToArray();
            this.Device = hidDeviceList.FirstOrDefault(d => d.VendorID == VendorId && d.ProductID == ProductId);

            Log.Information($"Connected to device {this.Device?.VendorID}");
            return this.Device != null;
        }

        public void StartReader()
        {
            Thread.Sleep(50);
            this.RunReader = true;

            if(this.Device != null && this.Thread?.IsAlive != true)
            {
                this.Thread = new Thread(
                () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    /* run your code here */
                    while(this.IsConnected && this.RunReader)
                    {
                        this.IsConnected = this.TryConnectDevice();
                        this.TryReadRaw();

                        if(!this.RunReader)
                        {
                            this.Thread?.Abort();
                        }
                    }
                });

                this.Thread.Start();
            }
        }

        public void TryReadRaw()
        {
            var requestValues = true;
            HidStream hidStream;

            var reportDescriptor = this.Device.GetReportDescriptor();

            try
            {
                var openConfiguration = new OpenConfiguration();
                openConfiguration.SetOption(OpenOption.Exclusive, true);

                if (this.Device.TryOpen(openConfiguration, out hidStream))
                {
                    Console.WriteLine("Opened device.");
                    hidStream.ReadTimeout = Timeout.Infinite;

                    using (hidStream)
                    {
                        var inputReportBuffer = new byte[this.Device.GetMaxInputReportLength()];
                        var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                        
                        // var inputParser = deviceItem.CreateDeviceItemInputParser();

                        inputReceiver.Start(hidStream);

                        int startTime = Environment.TickCount;
                        while (this.RunReader)
                        {
                            if (inputReceiver.WaitHandle.WaitOne(1000))
                            {

                                HidSharp.Utility.HidSharpDiagnostics.EnableTracing = true;
                                if (!inputReceiver.IsRunning) { break; } // Disconnected?

                                Report report;
                                while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                                {

                                    //this.VolumeController.MainVolume = (float)inputReportBuffer[4];
                                    this.VolumeController.MediaVolume = (float)inputReportBuffer[7];
                                    this.VolumeController.BrowserVolume = (float)inputReportBuffer[6];
                                    this.VolumeController.VoiceVolume = (float)inputReportBuffer[5];
                                    this.VolumeController.GamesVolume = (float)inputReportBuffer[4];
                                    // Parse the report if possible.
                                    // This will return false if (for example) the report applies to a different DeviceItem.

                                    //if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                                    //{
                                    //    WriteDeviceItemInputParserResult(inputParser);
                                    //}
                                 }
                            }

                            //if (requestValues)
                            //{
                            //    byte requestValuesByte = Convert.ToByte(requestValues);
                            //    var message = new byte[] { 0, 0, requestValuesByte };
                            //    try
                            //    {
                            //        hidStream.Write(message);
                            //        requestValues = false;
                            //    }
                            //    catch (TimeoutException timeoutException)
                            //    {
                            //        Log.Error(timeoutException.Message, timeoutException);
                            //        return;
                            //    }
                            //}
                        }
                    }

                    var test = "";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
                


            //    if (this.Device.TryOpen(out hidStream))
            //    {
            //        //hidStream.ReadTimeout = Timeout.Infinite;
            //        var inputReportBuffer = new byte[this.Device.GetMaxInputReportLength()];

            //        IAsyncResult ar = null;

            //        while (true)
            //        {
            //            //int byteCount = hidStream.Read(inputReportBuffer);

            //            if (ar == null)
            //            {
            //                Debug.WriteLine("Starting read");
            //                // var l = hidStream.Length;
            //                var cr = hidStream.CanRead;

            //                ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
            //                Debug.WriteLine("Read complete");
            //            }

            //            if (ar != null)
            //            {
            //                if (ar.IsCompleted)
            //                {
            //                    int byteCount = hidStream.EndRead(ar);
            //                    ar = null;

            //                    if (byteCount > 0)
            //                    {
            //                        if (inputReportBuffer[3] == 1)
            //                        {
            //                            this.VolumeController.ChangeMicrophoneMuteState();
            //                        }

            //                        this.VolumeController.MainVolume = (float)inputReportBuffer[4] / 100;
            //                        this.VolumeController.MediaVolume = (float)inputReportBuffer[5] / 100;
            //                        this.VolumeController.BrowserVolume = (float)inputReportBuffer[6] / 100;
            //                        this.VolumeController.VoiceVolume = (float)inputReportBuffer[7] / 100;
            //                        this.VolumeController.GamesVolume = (float)inputReportBuffer[8] / 100;
            //                        requestValues = false;
            //                    }

            //                    //byte muted = Convert.ToByte(VolumeController.IsMicrophoneMuted);
            //                    //byte requestValuesByte = Convert.ToByte(requestValues);

            //                    //var message = new byte[] { 0, muted, requestValuesByte };
            //                    //try
            //                    //{
            //                    //    hidStream.Write(message);
            //                    //}
            //                    //catch (TimeoutException timeoutException)
            //                    //{
            //                    //    Log.Error(timeoutException.Message, timeoutException);
            //                    //}
            //                }
            //                else
            //                {
            //                    ar.AsyncWaitHandle.WaitOne(100);
            //                }
            //            }


            //            Thread.SpinWait(20);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex, ex?.Message);
            //        //throw ex;
            //    }
            //    finally
            //    {
            //        hidStream.Flush();
            //        hidStream.Dispose();
            //        hidStream.Close();
            //    }
                
            //    return;
            //}
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
