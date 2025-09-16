namespace LCDonald.Core.Games;

/// <summary>
/// Duplicate for Burger white label only.
/// </summary>
public class BillyGiantEgg2 : BillyGiantEgg
{
    public override string ShortName => "eggcatch2";
#if BURGER
    public override string Name => "Barry's Treasure Hunt";
#endif
}
