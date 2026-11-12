using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ClockTimeZone), "clock")]
[JsonDerivedType(typeof(ZoneTimeZone), "zone")]
internal abstract class TimeZone
{
    private protected TimeZone()
    {
    }
}

internal sealed class ClockTimeZone : TimeZone
{
    internal static readonly ClockTimeZone Instance = new();

    private ClockTimeZone()
    {
    }
}

internal sealed class ZoneTimeZone : TimeZone
{
    internal ZoneTimeZone(string id)
    {
        Id = id;
    }

    public string Id { get; }
}
