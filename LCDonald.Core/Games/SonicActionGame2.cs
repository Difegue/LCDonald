namespace LCDonald.Core.Games;

/// <summary>
/// Duplicate for Burger white label only.
/// </summary>
public class SonicActionGame2 : SonicActionGame
{
    public override string ShortName => "sagame2";
#if BURGER
    public override string Name => "Burger Bard's Platform Adventure";
#endif
}
