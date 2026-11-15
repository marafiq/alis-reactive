namespace Alis.Reactive.InputsAndIds;

internal sealed class MvcNames
{
    internal MvcNames(string prefix, string name, string idOfTheName)
    {
        Prefix = prefix;
        Name = name;
        IdOfTheName = idOfTheName;
    }

    internal string Prefix { get; }

    internal string Name { get; }

    internal string IdOfTheName { get; }
}
