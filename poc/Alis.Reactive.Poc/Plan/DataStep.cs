using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal sealed class DataStep
{
    internal DataStep(string name, IReadOnlyList<string> requires, Absence absent)
    {
        Name = name;
        Requires = requires;
        Absent = absent;
    }

    public string Name { get; }

    public IReadOnlyList<string> Requires { get; }

    public Absence Absent { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(StartAbsence), "start")]
[JsonDerivedType(typeof(NothingAbsence), "nothing")]
[JsonDerivedType(typeof(UnboundAbsence), "unbound")]
internal abstract class Absence
{
    internal static readonly Absence Nothing = new NothingAbsence();
    internal static readonly Absence Unbound = new UnboundAbsence();

    private protected Absence()
    {
    }
}

internal sealed class StartAbsence : Absence
{
    internal StartAbsence(JsonElement value)
    {
        Value = value;
    }

    public JsonElement Value { get; }
}

internal sealed class NothingAbsence : Absence
{
    internal NothingAbsence()
    {
    }
}

internal sealed class UnboundAbsence : Absence
{
    internal UnboundAbsence()
    {
    }
}
