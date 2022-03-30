using System.Collections.Generic;

namespace LCDonald.Core.Layout
{
    public class MAMELayout
    {
        public Dictionary<string, MAMEElement> Elements { get; internal set; }
        public Dictionary<string, MAMEView> Views { get; internal set; }

        public MAMELayout()
        {
            Elements = new Dictionary<string, MAMEElement>();
            Views = new Dictionary<string, MAMEView>();
        }
    }
}