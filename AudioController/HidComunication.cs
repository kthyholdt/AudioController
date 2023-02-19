using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AudioController
{
    public class HidComunication : INotifyPropertyChanged
    {
        private const ushort ProductId = 0x0486;
        private const ushort VendorId = 0x16C0;

        public HidComunication(IVolumeController volumeController)
        {
            this.VolumeController = volumeController;
            this.VolumeController.SetDefaultRecordingEndpoint();

            EnumerateHidDevices();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IVolumeController VolumeController { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void EnumerateHidDevices()
        {
            var devices = await DeviceInformation.FindAllAsync();

            foreach (var d in devices)
            {
                if (d.Name == "Teensyduino RawHID Device")
                {
                    try
                    {
                        var device = await HidDevice.FromIdAsync(d.Id, FileAccessMode.ReadWrite);
                        if (device != null)
                        {
                            // Input reports contain data from the device.
                            device.InputReportReceived += async (sender, args) =>
                            {
                                HidInputReport inputReport = args.Report;

                                var bytes = new byte[4];
                                var bytes2 = new byte[4];

                                DataReader dataReader = DataReader.FromBuffer(inputReport.Data);
                                dataReader.ReadBytes(bytes);
                                dataReader.ReadBytes(bytes2);

                                this.VolumeController.GamesVolume = (float)bytes2[0];
                                this.VolumeController.VoiceVolume = (float)bytes2[1];
                                this.VolumeController.BrowserVolume = (float)bytes2[2];
                                this.VolumeController.MediaVolume = (float)bytes2[3];
                            };

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.ReportException(this, ex);
                    }
                }
            }
        }
    }
}