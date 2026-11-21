using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public abstract record BindingBase
{
    [JsonPropertyName("renamed_member")]
    public abstract string Renamed { get; init; }

    public abstract string Ignored { get; init; }
}
