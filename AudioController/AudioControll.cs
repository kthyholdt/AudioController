namespace AudioController
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using NAudio.CoreAudioApi;
    using NAudio.CoreAudioApi.Interfaces;

    using Serilog;

    public class AudioControl : INotifyPropertyChanged
    {
        private StringBuilder buffer = new StringBuilder();

        private char LF = (char)10;

        private double mainVolume;

        private double masterPeakValue;

        private SerialPort port;

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

            this.CreateGroups();

            if (this.port == null)
            {
                try
                {
                    this.port = new SerialPort("COM4", 9600);
                    this.port.Open();
                    this.port.DataReceived += this.port_DataReceived;
                    this.port.Write("G");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    //throw;
                }
            }

            if (port != null && this.port.IsOpen)
            {
                new Thread(
                    () =>
                    {
                        Thread.CurrentThread.IsBackground = true;

                        /* run your code here */
                        this.DefaultRecordingAudioEndpoint = this.DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                        this.SerialLoop();
                    }).Start();
            }

            Application.Current.Exit += this.CurrentOnExit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioSessionManager AudioSessionManager { get; set; }

        public MMDevice DefaultAudioEndpoint { get; set; }

        public MMDevice DefaultRecordingAudioEndpoint { get; set; }

        public MMDeviceEnumerator DeviceEnumerator { get; set; }

        public AudioSessionGroup GamesAudioSessionGroup { get; set; }

        public float GamesVolume
        {
            get => this.GamesAudioSessionGroup.Volume;
            set
            {
                this.GamesAudioSessionGroup.Volume = value;
                Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
            }
        }

        public double MainVolume
        {
            get => this.mainVolume;
            set
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

        public double MasterPeakValue
        {
            get => this.masterPeakValue;
            set
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

        public AudioSessionGroup MediaAudioSessionGroup { get; set; }

        public float MediaVolume
        {
            get => this.MediaAudioSessionGroup.Volume;
            set
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

        public AudioSessionGroup BrowserAudioSessionGroup { get; set; }

        public float BrowserVolume
        {
            get => this.BrowserAudioSessionGroup.Volume;
            set
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

        public AudioSessionGroup VoiceAudioSessionGroup { get; set; }

        public float VoiceVolume
        {
            get => this.VoiceAudioSessionGroup.Volume;
            set
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

        private void CurrentOnExit(object sender, ExitEventArgs e)
        {
            if ((this.port != null) && this.port.IsOpen)
            {
                this.port.Close();
            }
        }

        private void EvalLine(string line)
        {
            try
            {
                if (line.StartsWith("P1"))
                {
                    var vol = line.Split(':')[1];
                    if (double.TryParse(vol, out var volume))
                    {
                        this.MainVolume = (float)(volume / 100);
                    }
                }

                if (line.StartsWith("P2"))
                {
                    var vol = line.Split(':')[1];
                    if (double.TryParse(vol, out var volume))
                    {
                        this.MediaVolume = (float)(volume / 100);
                    }
                }

                if (line.StartsWith("P3"))
                {
                    var vol = line.Split(':')[1];
                    if (double.TryParse(vol, out var volume))
                    {
                        this.BrowserVolume = (float)(volume / 100);
                    }
                }

                if (line.StartsWith("P4"))
                {
                    var vol = line.Split(':')[1];
                    if (double.TryParse(vol, out var volume))
                    {
                        this.VoiceVolume = (float)(volume / 100);
                    }
                }

                if (line.StartsWith("P5"))
                {
                    var vol = line.Split(':')[1];
                    if (double.TryParse(vol, out var volume))
                    {
                        this.GamesVolume = (float)(volume / 100);
                    }
                }

                if (line.StartsWith("M"))
                {
                    this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute = !this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        private void OnAudioSessionCreated(object sender, IAudioSessionControl newsession)
        {
            this.CreateGroups();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = this.port.ReadExisting();

            foreach (char c in data)
            {
                if (c == this.LF)
                {
                    this.buffer.Append(c);

                    var currentLine = this.buffer.ToString();
                    this.buffer.Clear();

                    this.EvalLine(currentLine);
                }
                else
                {
                    this.buffer.Append(c);
                }
            }
        }

        private void PortWrite(string message)
        {
            this.port.Write(message);
        }

        private void SerialLoop()
        {
            while (true)
            {
                if ((this.port != null) && this.port.IsOpen)
                {
                    if (this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute)
                    {
                        this.PortWrite("1");
                    }
                    else
                    {
                        this.PortWrite("0");
                    }
                }

                Thread.SpinWait(50);
            }
        }

        private void UpdateValues()
        {
            this.OnPropertyChanged(nameof(this.MasterPeakValue));
        }
    }
}