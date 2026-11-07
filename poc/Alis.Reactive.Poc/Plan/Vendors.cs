using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal sealed class Vendor
{
    internal Vendor(FnExpression locate, Listening listen)
        : this(locate, listen, [])
    {
    }

    internal Vendor(FnExpression locate, Listening listen, IReadOnlyList<PluginLocator> lookups)
    {
        Locate = locate;
        Listen = listen;
        Lookups = lookups;
    }

    public Expression Locate { get; }

    public Listening Listen { get; }

    internal IReadOnlyList<PluginLocator> Lookups { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(MethodListening), "method")]
[JsonDerivedType(typeof(AssignmentListening), "assignment")]
[JsonDerivedType(typeof(ReturnedListening), "returned")]
internal abstract class Listening
{
    private protected Listening()
    {
    }
}

internal sealed class MethodListening : Listening
{
    internal MethodListening(string subscribe, string unsubscribe)
    {
        Subscribe = subscribe;
        Unsubscribe = unsubscribe;
    }

    public string Subscribe { get; }

    public string Unsubscribe { get; }
}

internal sealed class AssignmentListening : Listening
{
    internal AssignmentListening()
    {
    }
}

internal sealed class ReturnedListening : Listening
{
    internal ReturnedListening(string subscribe)
    {
        Subscribe = subscribe;
    }

    public string Subscribe { get; }
}
