namespace Alis.Reactive.Schema;

internal sealed class Culture
{
    internal Culture(string name, string collation, string decimalSeparator, string groupSeparator, NumberText integer, NumberText fraction)
    {
        Name = name;
        Collation = collation;
        Decimal = decimalSeparator;
        Group = groupSeparator;
        Integer = integer;
        Fraction = fraction;
    }

    public string Name { get; }

    public string Collation { get; }

    public string Decimal { get; }

    public string Group { get; }

    public NumberText Integer { get; }

    public NumberText Fraction { get; }
}

internal sealed class NumberText
{
    internal NumberText(bool group, bool exponent)
    {
        Group = group;
        Exponent = exponent;
    }

    public bool Group { get; }

    public bool Exponent { get; }
}
