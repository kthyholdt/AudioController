using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Serilog;

namespace AudioController
{
    public class AudioControl : INotifyPropertyChanged, IVolumeController
    {
        private double mainVolume;

        private double masterPeakValue;

        double deltaVolume = 0.01;

        private HidComunication device;

        public AudioControl()
        {
            this.MediaAudioSessionGroup = new AudioSessionGroup();
            this.BrowserAudioSessionGroup = new AudioSessionGroup();
            this.VoiceAudioSessionGroup = new AudioSessionGroup();
            this.GamesAudioSessionGroup = new AudioSessionGroup();

            this.DeviceEnumerator = new MMDeviceEnumerator();
            this.DefaultAudioEndpoint = this.DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            this.AudioSessionManager = this.DefaultAudioEndpoint.AudioSessionManager;
            this.MainVolume = this.DefaultAudioEndpoint.AudioEndpointVolume.MasterVolumeLevelScalar;
            this.AudioSessionManager.OnSessionCreated += this.OnAudioSessionCreated;
            this.SetDefaultRecordingEndpoint();

            this.CreateGroups();

            device = new HidComunication(this);

            //new Thread(
            //        () =>
            //        {
            //            Thread.CurrentThread.IsBackground = true;

            //            /* run your code here */
            //            this.DefaultRecordingAudioEndpoint = this.DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            //            device.TryReadRaw();
            //        }).Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioSessionManager AudioSessionManager { get; set; }

        public MMDevice DefaultAudioEndpoint { get; set; }

        public MMDevice DefaultRecordingAudioEndpoint { get; set; }

        public MMDeviceEnumerator DeviceEnumerator { get; set; }

        public AudioSessionGroup GamesAudioSessionGroup { get; set; }

        public void SetDefaultRecordingEndpoint()
        {
            this.DefaultRecordingAudioEndpoint = this.DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
        }

        public float GamesVolume
        {
            get => this.GamesAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.GamesAudioSessionGroup.Volume) >= 0.02)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    this.GamesAudioSessionGroup.Volume = volume;
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        public double MainVolume
        {
            get => this.mainVolume;
            set
            {
                if(Math.Abs(value - this.mainVolume) >= deltaVolume)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }
                    this.mainVolume = volume;
                    this.DefaultAudioEndpoint.AudioEndpointVolume.MasterVolumeLevelScalar = (float)volume;
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        public double MasterPeakValue
        {
            get => this.masterPeakValue;
            set
            {
                if (Math.Abs(value - this.masterPeakValue) >= deltaVolume)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    this.masterPeakValue = volume;
                    Application.Current?.Dispatcher?.Invoke(this.UpdateValues);
                }
            }
        }

        public AudioSessionGroup MediaAudioSessionGroup { get; set; }

        public float MediaVolume
        {
            get => this.MediaAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.MediaAudioSessionGroup.Volume) >= deltaVolume)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    this.MediaAudioSessionGroup.Volume = volume;
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        public AudioSessionGroup BrowserAudioSessionGroup { get; set; }

        public float BrowserVolume
        {
            get => this.BrowserAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.BrowserAudioSessionGroup.Volume) >= deltaVolume)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    this.BrowserAudioSessionGroup.Volume = volume;
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        public AudioSessionGroup VoiceAudioSessionGroup { get; set; }

        public bool IsMicrophoneMuted => this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute;

        public float VoiceVolume
        {
            get => this.VoiceAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.VoiceAudioSessionGroup.Volume) >= deltaVolume)
                {
                    var volume = value;
                    if (volume < 0)
                    {
                        volume = 0;
                    }

                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    this.VoiceAudioSessionGroup.Volume = volume;
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string GetApplicationName(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private void AddToAudioSessionGroup(AudioSessionControl audioSessionControl)
        {
            var name = GetApplicationName((int)audioSessionControl.GetProcessID);

            if (name.Equals("spotify", StringComparison.CurrentCultureIgnoreCase))
            {
                this.MediaAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
            else if (name.Equals("chrome", StringComparison.CurrentCultureIgnoreCase))
            {
                this.BrowserAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
            else if (name.Equals("skype", StringComparison.CurrentCultureIgnoreCase)
                     || name.Equals("ts3client_win64", StringComparison.CurrentCultureIgnoreCase)
                     || name.Equals("lync", StringComparison.CurrentCultureIgnoreCase))
            {
                this.VoiceAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
            else
            {
                this.GamesAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
        }

        public void ChangeMicrophoneMuteState()
        {
            this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute = !this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute;
        }

        private void CreateGroups()
        {
            this.DefaultAudioEndpoint.AudioSessionManager.RefreshSessions();

            this.GamesAudioSessionGroup.Clear();
            this.MediaAudioSessionGroup.Clear();
            this.VoiceAudioSessionGroup.Clear();

            var sessions = this.AudioSessionManager.Sessions;
            for (var i = 0; i < sessions.Count; i++)
            {
                this.AddToAudioSessionGroup(sessions[i]);
            }
        }

        private void OnAudioSessionCreated(object sender, IAudioSessionControl newsession)
        {
            this.CreateGroups();
        }

        private void UpdateValues()
        {
            this.OnPropertyChanged(nameof(this.MasterPeakValue));
        }
    }
}