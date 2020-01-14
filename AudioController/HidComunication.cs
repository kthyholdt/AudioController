using HidSharp;
using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace AudioController
{
    public class HidComunication
    {
        private const int VendorId = 0x16C0;
        private const int ProductId = 0x0486;

        public HidComunication(IVolumeController volumeController)
        {
            DeviceList.Local.Changed += DeviceListChangedHandler;
            var hidDeviceList = DeviceList.Local.GetHidDevices().ToArray();
            this.Device = hidDeviceList.FirstOrDefault(d => d.VendorID == VendorId && d.ProductID == ProductId);
            VolumeController = volumeController;
            StartReader();
        }

        private HidDevice Device { get; set; }

        public bool IsConnected => this.Device != null;

        private Thread Thread { get; set; }

        public IVolumeController VolumeController { get; }

        private void DeviceListChangedHandler(object sender, DeviceListChangedEventArgs e)
        {
            var list = DeviceList.Local;
            var hidDeviceList = list.GetHidDevices().ToArray();
            var device = hidDeviceList.FirstOrDefault(d => d.VendorID == VendorId && d.ProductID == ProductId);
            if(this.Device != device)
            {
                this.Device = device;
                if(this.Device != null)
                {
                    StartReader();
                }
            }
        }

        private void StartReader()
        {
            if(this.IsConnected && Thread?.IsAlive != true)
            {
                Thread = new Thread(
                () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    VolumeController.SetDefaultRecordingEndpoint();
                    /* run your code here */
                    TryReadRaw();
                });

                Thread.Start();
            }
        }

        public void TryReadRaw()
        {
            HidStream hidStream;
            if (this.Device.TryOpen(out hidStream))
            {
                try
                {
                    hidStream.ReadTimeout = Timeout.Infinite;
                    var inputReportBuffer = new byte[this.Device.GetMaxInputReportLength()];

                    IAsyncResult ar = null;

                    while (true)
                    {
                        //int byteCount = hidStream.Read(inputReportBuffer);

                        if (ar == null)
                        {
                            ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                        }

                        if (ar != null)
                        {
                            if (ar.IsCompleted)
                            {
                                int byteCount = hidStream.EndRead(ar);
                                ar = null;

                                if (byteCount > 0)
                                {
                                    if (inputReportBuffer[3] == 1)
                                    {
                                        VolumeController.ChangeMicrophoneMuteState();
                                    }

                                    VolumeController.MainVolume = (float)inputReportBuffer[4] / 100;
                                    VolumeController.MediaVolume = (float)inputReportBuffer[5] / 100;
                                    VolumeController.BrowserVolume = (float)inputReportBuffer[6] / 100;
                                    VolumeController.VoiceVolume = (float)inputReportBuffer[7] / 100;
                                    VolumeController.GamesVolume = (float)inputReportBuffer[8] / 100;
                                }
                            }
                            else
                            {
                                ar.AsyncWaitHandle.WaitOne(100);
                            }
                        }

                        byte muted = Convert.ToByte(VolumeController.IsMicrophoneMuted);
                        var message = new byte[] { 0, muted };
                        hidStream.Write(message);
                        Thread.SpinWait(20);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    //throw ex;
                }
                finally
                {
                    hidStream.Flush();
                    hidStream.Dispose();
                    hidStream.Close();
                }
                
                return;
            }
        }
    }
}
