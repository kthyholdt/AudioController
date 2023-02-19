namespace AudioController
{
    public interface IVolumeController
    {
        float BrowserVolume { set; }

        float GamesVolume { set; }

        bool IsMicrophoneMuted { get; }

        double MainVolume { set; }

        double MasterPeakValue { set; }

        float MediaVolume { set; }

        float VoiceVolume { set; }

        void ChangeMicrophoneMuteState();

        void SetDefaultRecordingEndpoint();
    }
}