namespace LCDonald.Core.Games;

/// <summary>
/// Duplicate for Burger white label only.
/// </summary>
public class KnucklesTreasureHunt2 : KnucklesTreasureHunt
{
    public override string ShortName => "treasure2";
#if BURGER
    public override string Name => "Halmon's Egg Hunt";
#endif
}
