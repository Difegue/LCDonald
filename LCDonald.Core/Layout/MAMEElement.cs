namespace LCDonald.Core.Layout
{
    public record MAMEElement
    {
        public string Name { get; init; } = default!;
        public MAMEImage Image { get; init; } = default!;
    }

    public record MAMEElementRef
    {
        public string Ref { get; init; } = default!;
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

    }
}