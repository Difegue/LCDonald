using System.Collections.Generic;

namespace LCDonald.Core.Layout
{
    public record MAMEView
    {
        public string Name { get; init; }
        public int ScreenX { get; init; }
        public int ScreenY { get; init; }
        public int ScreenWidth { get; init; }
        public int ScreenHeight { get; init; }
        public int ScreenIndex { get; init; }

        public List<MAMEElementRef> Elements { get; init; } = new List<MAMEElementRef>();

        public override string ToString() => Name;
    }
}