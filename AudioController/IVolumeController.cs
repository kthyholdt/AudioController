namespace AudioController
{
    public interface IVolumeController
    {
        double MainVolume { set; }

        double MasterPeakValue { set; }

        float GamesVolume { set; }

        float MediaVolume { set; }

        float BrowserVolume { set; }

        float VoiceVolume { set; }

        bool IsMicrophoneMuted { get; }

        void ChangeMicrophoneMuteState();
        void SetDefaultRecordingEndpoint();
    }
}
