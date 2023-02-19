using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AudioController
{
    using System;
    using System.Linq;

    public class AudioSessionGroup
    {
        private bool mute;
        private float volume;

        public AudioSessionGroup()
        {
            this.AudioSessionControls = new List<AudioSessionControl>();
            this.mute = false;
        }

        public bool Mute
        {
            get => this.mute;
            set
            {
                this.mute = value;

                foreach (var audioSessionControl in this.AudioSessionControls.ToList())
                {
                    if (audioSessionControl.State == AudioSessionState.AudioSessionStateActive)
                    {
                        audioSessionControl.SimpleAudioVolume.Mute = value;
                    }
                }
            }
        }

        public float Volume
        {
            get => this.volume;
            set
            {
                this.volume = value;

                if (this.volume < 0)
                {
                    this.volume = 0;
                }

                if (this.volume > 100)
                {
                    this.volume = 100;
                }

                foreach (var audioSessionControl in this.AudioSessionControls.ToList())
                {
                    try
                    {
                        audioSessionControl.SimpleAudioVolume.Volume = this.CalcVolume();
                    }
                    catch (Exception)
                    {
                        Serilog.Log.Error($"audioSessionControl?.DisplayName: {audioSessionControl?.DisplayName}");
                        throw;
                    }
                }
            }
        }

        private List<AudioSessionControl> AudioSessionControls { get; set; }

        public void AddAudioSessionControl(AudioSessionControl audioSessionControl)
        {
            audioSessionControl.SimpleAudioVolume.Volume = this.CalcVolume();
            audioSessionControl.SimpleAudioVolume.Mute = this.mute;
            this.AudioSessionControls.Add(audioSessionControl);
        }

        public void Clear()
        {
            this.AudioSessionControls.Clear();
        }

        private float CalcVolume()
        {
            return (float)Math.Pow(this.volume / 100, 3);
        }
    }
}