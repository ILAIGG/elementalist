using System;
using System.Collections.Generic;

public sealed class ElementalCharge
{
    public ElementalCharge(Element element, float amount, float duration)
    {
        Element = element;
        Amount = amount;
        RemainingDuration = duration;
    }

    public Element Element { get; }
    public float Amount { get; private set; }
    public float RemainingDuration { get; private set; }

    public void Add(float amount, float duration)
    {
        Amount += amount;
        RemainingDuration = duration;
    }

    public void Update(double delta)
    {
        RemainingDuration -= (float)delta;
    }
}

public sealed class ElementalAccumulator
{
    public const float DefaultDuration = 5f;

    private readonly Dictionary<Element, ElementalCharge> _charges = new();

    public event Action<Element, float> ElementApplied;

    public void Apply(Element element, float amount, float duration = DefaultDuration)
    {
        if (element == Element.Neutral || amount <= 0f || duration <= 0f)
            return;

        if (_charges.TryGetValue(element, out ElementalCharge charge))
            charge.Add(amount, duration);
        else
            _charges[element] = new ElementalCharge(element, amount, duration);

        ElementApplied?.Invoke(element, amount);
    }

    public float GetAmount(Element element)
    {
        return _charges.TryGetValue(element, out ElementalCharge charge)
            ? charge.Amount
            : 0f;
    }

    public bool Has(Element element)
    {
        return GetAmount(element) > 0f;
    }

    public void Consume(Element element)
    {
        _charges.Remove(element);
    }

    public void Update(double delta)
    {
        List<Element> expiredElements = new();
        foreach (KeyValuePair<Element, ElementalCharge> entry in _charges)
        {
            entry.Value.Update(delta);
            if (entry.Value.RemainingDuration <= 0f)
                expiredElements.Add(entry.Key);
        }

        foreach (Element element in expiredElements)
            _charges.Remove(element);
    }
}