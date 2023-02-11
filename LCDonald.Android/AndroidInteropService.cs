using Android.App;
using LCDonald.Core.Controller;
using System.IO;

namespace LCDonald.Android
{
    internal class AndroidInteropService: IInteropService
    {
        public Stream GetGameAsset(string game, string assetName)
        {
            var assetManager = Application.Context.Assets;
            var assetStream = assetManager.Open(Path.Combine("GameAssets", game, assetName));

            // copy stream and return copy
            var ms = new MemoryStream();
            assetStream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}