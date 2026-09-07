using System.Collections.Generic;

public static class ElementalChart
{
    private static readonly Dictionary<(Element Attack, Element Defense), float> DamageMultipliers = new()
    {
        [(Element.Fire, Element.Water)] = 0.5f,
        [(Element.Fire, Element.Plant)] = 1.5f
    };

    public static float GetDamageMultiplier(Element attackElement, Element defenseElement)
    {
        return DamageMultipliers.TryGetValue((attackElement, defenseElement), out float multiplier)
            ? multiplier
            : 1f;
    }
}