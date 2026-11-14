using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class HeldForm
{
    private readonly AppJson _json;

    private HeldForm(AppJson json)
    {
        _json = json;
    }

    internal HeldForm Computed => Live(_json);

    internal static HeldForm Live(AppJson json) => new(json);

    internal LitExpression Beside<TValue>(TValue value) => Literals.Of(value, _json);

    internal bool WritesAsAScalar<TValue>() => _json.LiveForm.GetTypeInfo(typeof(TValue)).Kind == JsonTypeInfoKind.None;
}
