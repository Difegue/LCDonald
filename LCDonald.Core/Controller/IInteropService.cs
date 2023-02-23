using LCDonald.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LCDonald.Core.Controller
{
    
    /// <summary>
    /// Platform-specific methods.
    /// </summary>
    public interface IInteropService
    {
        Stream GetGameAsset(string game, string assetName);
        ISoundStream PlayAudio(string game, string assetName, float v);
    }

    public interface ISoundStream
    {
        bool IsPlaying { get; }
        void Dispose();
        void Stop();
    }
}
