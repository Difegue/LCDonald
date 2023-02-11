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
    }
}
