namespace LCDonald.Core.Games;

/// <summary>
/// Duplicate for Burger white label only.
/// </summary>
public class KnucklesTreasureHunt2 : KnucklesTreasureHunt
{
    public override string ShortName => "treasurehunt";
#if BURGER
    public override string Name => "Bard's Burger Hunt";
#endif
}
