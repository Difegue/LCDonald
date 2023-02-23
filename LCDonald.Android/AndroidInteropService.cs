using Android.App;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using Java.Util;
using LCDonald.Core.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LCDonald.Android
{
    internal class AndroidInteropService: IInteropService
    {
        private SoundPool? _soundPool;
        private Dictionary<string, int> _loadedSounds;

        public AndroidInteropService()
        {
            _loadedSounds = new();
            
            var soundPoolBuilder = new SoundPool.Builder();
            
            var attributesBuilder = new AudioAttributes.Builder();
            attributesBuilder.SetUsage(AudioUsageKind.Game);
            attributesBuilder.SetContentType(AudioContentType.Sonification);

            soundPoolBuilder.SetAudioAttributes(attributesBuilder.Build());
            soundPoolBuilder.SetMaxStreams(10);

            _soundPool = soundPoolBuilder.Build();
        }
        
        public System.IO.Stream GetGameAsset(string game, string assetName)
        {
            var assetManager = Application.Context.Assets;
            var assetStream = assetManager.Open(GetAssetPath(game, assetName));

            // copy stream and return copy
            var ms = new MemoryStream();
            assetStream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
        
        public ISoundStream PlayAudio(string game, string assetName, float v)
        {
            var identifier = GetAssetPath(game, assetName);
            var assetManager = Application.Context.Assets;

            if (_loadedSounds.ContainsKey(identifier))
            {
                var soundId = _loadedSounds[identifier];
                var streamId = _soundPool?.Play(soundId, v, v, 1, 0, 1);
                return new AndroidSoundStream(_soundPool, streamId ?? -1);
            }
            else
            {
                var asset = assetManager.OpenFd(identifier);
                var soundId = _soundPool?.Load(asset, 1) ?? -1;

                // Wait for the sound to load because we have no callbacks lmao
                // This is inefficient and won't always work with longer sounds/slow hardware, but at least it only needs to happen once..
                Thread.Sleep(500); 
                
                _loadedSounds.Add(identifier, soundId);
                var streamId = _soundPool?.Play(soundId, v, v, 1, 0, 1);

                return new AndroidSoundStream(_soundPool, streamId ?? -1);
            }
        }

        private static string GetAssetPath(string game, string assetName)
        {
            if (assetName.StartsWith("../"))
            {
                // If the assetname isn't in the game folder walk back ourselves because Android is very stupid
                game = "";
                assetName = assetName.Replace("../", "");
            }

            return Path.Combine("GameAssets", game, assetName);
        }
    }

    internal class AndroidSoundStream : ISoundStream
    {
        private SoundPool? _soundPool;
        private int _streamId;

        public AndroidSoundStream(SoundPool? soundPool, int streamId)
        {
            _soundPool = soundPool;
            _streamId = streamId;
        }

        public bool IsPlaying => false; // Unsupported by SoundPool
        public void Dispose() { } //_soundPool?.Unload(_soundId);
        public void Stop() => _soundPool?.Stop(_streamId);
    }
}