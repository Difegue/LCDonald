using LCDonald.Core.Controller;
using System.IO;
using System.Reflection;

namespace LCDonald.Desktop
{
    internal class DesktopInteropService : IInteropService
    {
        private string _gameAssetFolder;
        
        public DesktopInteropService()
        {
            var appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _gameAssetFolder = Path.Combine(appFolder, "GameAssets");
        }

        public Stream GetGameAsset(string game, string assetName) =>  File.OpenRead(Path.Combine(_gameAssetFolder, game, assetName));
    }
}