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
    internal static readonly MethodListening EventTarget = new("addEventListener", "removeEventListener");

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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(LivingRelease), "living")]
[JsonDerivedType(typeof(HeldRelease), "held")]
internal abstract class Release
{
    private protected Release()
    {
    }
}

internal sealed class LivingRelease : Release
{
    private LivingRelease()
    {
    }

    internal static LivingRelease Instance { get; } = new();
}

internal sealed class HeldRelease : Release
{
    internal HeldRelease(string vendor, IReadOnlyList<PathStep> element)
    {
        Vendor = vendor;
        Element = element;
    }

    public string Vendor { get; }

    public IReadOnlyList<PathStep> Element { get; }
}

internal sealed class ReturnedListening : Listening
{
    internal ReturnedListening(string subscribe)
    {
        Subscribe = subscribe;
    }

    public string Subscribe { get; }
}
