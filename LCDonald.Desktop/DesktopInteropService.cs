using LCDonald.Core.Controller;
using SharpAudio;
using SharpAudio.Codec;
using System;
using System.IO;
using System.Reflection;

namespace LCDonald.Desktop
{
    internal class DesktopInteropService : IInteropService
    {
        private string _gameAssetFolder;
        private readonly AudioEngine _gameAudio;

        public DesktopInteropService()
        {
            var appFolder = AppContext.BaseDirectory;
            _gameAssetFolder = Path.Combine(appFolder, "GameAssets");

            _gameAudio = AudioEngine.CreateDefault();

        }

        public Stream GetGameAsset(string game, string assetName) => File.OpenRead(Path.Combine(_gameAssetFolder, game, assetName));

        public ISoundStream PlayAudio(string game, string assetName, float v)
        {
            var soundFile = GetGameAsset(game, assetName);
            var soundStream = new SoundStream(soundFile, _gameAudio)
            {
                Volume = v
            };
            soundStream.Play();

            return new SharpAudioWrapper(soundStream);
        }
    }

    internal class SharpAudioWrapper : ISoundStream
    {
        private SoundStream _soundStream;
        public SharpAudioWrapper(SoundStream soundStream)
        {
            _soundStream = soundStream;
        }

        public bool IsPlaying => _soundStream.IsPlaying;
        public void Dispose() => _soundStream.Dispose();
        public void Stop() => _soundStream.Stop();
    }
}