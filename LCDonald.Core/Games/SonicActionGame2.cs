namespace LCDonald.Core.Games;

/// <summary>
/// Duplicate for Burger white label only.
/// </summary>
public class SonicActionGame2 : SonicActionGame
{
#if BURGER
    public override string ShortName => "platform2";
    public override string Name => "Burger Bard's Platform Adventure";
#endif
}
