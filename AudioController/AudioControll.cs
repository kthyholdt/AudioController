using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Serilog;

namespace AudioController
{
    public class AudioControl : INotifyPropertyChanged, IVolumeController
    {
        private const double DeltaVolume = 1;

        private string _currentControl;
        private double _currentVolume;
        private string _errorMessage;
        private bool _isErrorVisible;
        private bool _isPopupOpen;
        private Timer errorPopupTimer;
        private double mainVolume;
        private double masterPeakValue;
        private Timer volumePopupTimer;

        public AudioControl()
        {
            Application.Current.Exit += this.OnExit;
            volumePopupTimer = new Timer();
            volumePopupTimer.Interval = 2000;
            volumePopupTimer.AutoReset = false;
            volumePopupTimer.Elapsed += this.VolumeTimerElapsed;

            errorPopupTimer = new Timer();
            errorPopupTimer.Interval = 5000;
            errorPopupTimer.AutoReset = false;
            errorPopupTimer.Elapsed += this.ErrorTimerElapsed;
            ErrorReporter.ErrorOccured += this.ErrorReporterErrorOccured;

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
            this.MainVolume = Properties.Settings.Default.Volume1;
            this.MediaVolume = Properties.Settings.Default.Volume2;
            this.BrowserVolume = Properties.Settings.Default.Volume3;
            this.VoiceVolume = Properties.Settings.Default.Volume4;
            this.GamesVolume = Properties.Settings.Default.Volume5;

            this.Device = new HidComunication(this);

            this.PropertyChanged += this.AudioControlPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioSessionManager AudioSessionManager { get; set; }

        public AudioSessionGroup BrowserAudioSessionGroup { get; set; }

        public float BrowserVolume
        {
            get => this.BrowserAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.BrowserAudioSessionGroup.Volume) > DeltaVolume)
                {
                    try
                    {
                        this.BrowserAudioSessionGroup.Volume = value;
                        Log.Debug($"Browser Volume changed to {value}.");
                        Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
        }

        public string CurrentControl
        {
            get { return _currentControl; }
            set
            {
                _currentControl = value;
                OnPropertyChanged();
            }
        }

        public double CurrentVolume
        {
            get { return _currentVolume; }
            set
            {
                _currentVolume = value;
                OnPropertyChanged();
            }
        }

        public MMDevice DefaultAudioEndpoint { get; set; }

        public MMDevice DefaultRecordingAudioEndpoint { get; set; }

        public HidComunication Device { get; set; }

        public MMDeviceEnumerator DeviceEnumerator { get; set; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public AudioSessionGroup GamesAudioSessionGroup { get; set; }

        public float GamesVolume
        {
            get => this.GamesAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - this.GamesAudioSessionGroup.Volume) > DeltaVolume)
                {
                    try
                    {
                        this.GamesAudioSessionGroup.Volume = value;
                        Log.Debug($"Games Volume changed to {value}.");
                        Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
        }

        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set
            {
                _isErrorVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsMicrophoneMuted => this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute;

        public bool IsPopupVisible
        {
            get { return _isPopupOpen && !IsErrorVisible; }
            set
            {
                _isPopupOpen = value;
                OnPropertyChanged();
            }
        }

        public double MainVolume
        {
            get => this.mainVolume;
            set
            {
                if (Math.Abs(value - this.mainVolume) > 0)
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
                    Log.Debug($"Main Volume changed to {value}.");
                    Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                }
            }
        }

        public double MasterPeakValue
        {
            get => this.masterPeakValue;
            set
            {
                if (Math.Abs(value - this.masterPeakValue) > 0)
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
                if (Math.Abs(value - this.MediaAudioSessionGroup.Volume) > DeltaVolume)
                {
                    try
                    {
                        this.MediaAudioSessionGroup.Volume = value;
                        Log.Debug($"Media Volume changed to {value}.");
                        Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
        }

        public AudioSessionGroup VoiceAudioSessionGroup { get; set; }

        public float VoiceVolume
        {
            get => this.VoiceAudioSessionGroup.Volume;
            set
            {
                if (Math.Abs(value - VoiceAudioSessionGroup.Volume) > DeltaVolume)
                {
                    try
                    {
                        this.VoiceAudioSessionGroup.Volume = value;
                        Log.Debug($"Voice Volume changed to {value}.");
                        Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged());
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
        }

        public void ChangeMicrophoneMuteState()
        {
            this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute = !this.DefaultRecordingAudioEndpoint.AudioEndpointVolume.Mute;
        }

        public void SetDefaultRecordingEndpoint()
        {
            this.DefaultRecordingAudioEndpoint = this.DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
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
                     || name.Equals("lync", StringComparison.CurrentCultureIgnoreCase)
                     || name.Equals("discord", StringComparison.CurrentCultureIgnoreCase)
                     || name.Equals("teams", StringComparison.CurrentCultureIgnoreCase))
            {
                this.VoiceAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
            else
            {
                this.GamesAudioSessionGroup.AddAudioSessionControl(audioSessionControl);
            }
        }

        private void AudioControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.GamesVolume))
            {
                this.IsPopupVisible = true;
                this.CurrentControl = "Games";
                this.CurrentVolume = this.GamesVolume;

                volumePopupTimer.Stop();
                volumePopupTimer.Start();
            }
            else if (e.PropertyName == nameof(this.MediaVolume))
            {
                this.IsPopupVisible = true;
                this.CurrentControl = "Media";
                this.CurrentVolume = this.MediaVolume;

                volumePopupTimer.Stop();
                volumePopupTimer.Start();
            }
            else if (e.PropertyName == nameof(this.BrowserVolume))
            {
                this.IsPopupVisible = true;
                this.CurrentControl = "Browser";
                this.CurrentVolume = this.BrowserVolume;

                volumePopupTimer.Stop();
                volumePopupTimer.Start();
            }
            else if (e.PropertyName == nameof(this.VoiceVolume))
            {
                this.IsPopupVisible = true;
                this.CurrentControl = "Voice";
                this.CurrentVolume = this.VoiceVolume;

                volumePopupTimer.Stop();
                volumePopupTimer.Start();
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

        private void ErrorReporterErrorOccured(object sender, System.IO.ErrorEventArgs e)
        {
            this.ShowErrorMessage(e.GetException().Message);
        }

        private void ErrorTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsErrorVisible = false;
            });
        }

        private void OnAudioSessionCreated(object sender, IAudioSessionControl newsession)
        {
            this.CreateGroups();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Volume1 = this.MainVolume;
            Properties.Settings.Default.Volume2 = this.MediaVolume;
            Properties.Settings.Default.Volume3 = this.BrowserVolume;
            Properties.Settings.Default.Volume4 = this.VoiceVolume;
            Properties.Settings.Default.Volume5 = this.GamesVolume;

            Properties.Settings.Default.Save();
        }

        private void ShowErrorMessage(string message)
        {
            this.ErrorMessage = message;
            this.IsErrorVisible = true;

            errorPopupTimer.Stop();
            errorPopupTimer.Start();
        }

        private void UpdateValues()
        {
            this.OnPropertyChanged(nameof(this.MasterPeakValue));
        }

        private void VolumeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsPopupVisible = false;
            });
        }
    }
}