using System;

public enum ElementalReaction
{
    None,
    Vaporization,
    Freezing
}

public static class ElementalReactionResolver
{
    public static bool TryResolve(
        ElementalAccumulator accumulator,
        Element appliedElement,
        out ElementalReaction reaction,
        out float reactionDamage)
    {
        reaction = ElementalReaction.None;
        reactionDamage = 0f;

        Element otherElement = appliedElement switch
        {
            Element.Fire when accumulator.Has(Element.Water) => Element.Water,
            Element.Water when accumulator.Has(Element.Fire) => Element.Fire,
            Element.Water when accumulator.Has(Element.Ice) => Element.Ice,
            Element.Ice when accumulator.Has(Element.Water) => Element.Water,
            _ => Element.Neutral
        };

        if (otherElement == Element.Neutral)
            return false;

        float appliedAmount = accumulator.GetAmount(appliedElement);
        float otherAmount = accumulator.GetAmount(otherElement);
        float consumedAmount = MathF.Min(appliedAmount, otherAmount);

        if ((appliedElement == Element.Fire && otherElement == Element.Water)
            || (appliedElement == Element.Water && otherElement == Element.Fire))
        {
            reaction = ElementalReaction.Vaporization;
            reactionDamage = consumedAmount * 2f;
        }
        else
        {
            reaction = ElementalReaction.Freezing;
        }

        return true;
    }
}