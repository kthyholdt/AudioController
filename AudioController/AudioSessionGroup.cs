﻿using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AudioController
{
    using System;
    using System.Linq;

    public class AudioSessionGroup
    {
        private float volume;
        private bool mute;

        public AudioSessionGroup()
        {
            this.AudioSessionControls = new List<AudioSessionControl>();
            this.mute = false;
        }

        public float Volume
        {
            get => this.volume;
            set
            {
                this.volume = value;

                foreach (var audioSessionControl in this.AudioSessionControls.ToList())
                {
                    audioSessionControl.SimpleAudioVolume.Volume = this.CalcVolume(this.volume);
                }
            }
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

        private List<AudioSessionControl> AudioSessionControls { get; set; }

        public void AddAudioSessionControl(AudioSessionControl audioSessionControl)
        {
            audioSessionControl.SimpleAudioVolume.Volume = this.CalcVolume(this.volume);
            audioSessionControl.SimpleAudioVolume.Mute = this.mute;
            this.AudioSessionControls.Add(audioSessionControl);
        }

        public void Clear()
        {
            this.AudioSessionControls.Clear();
        }

        private float CalcVolume(float value)
        {
            return (float)Math.Pow(this.volume, 3);
        }
    }
}
