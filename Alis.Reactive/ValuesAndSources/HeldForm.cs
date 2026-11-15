using System;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class HeldForm
{
    private readonly AppJson _json;
    private readonly bool _ownForm;

    private HeldForm(AppJson json, bool ownForm)
    {
        _json = json;
        _ownForm = ownForm;
    }

    internal HeldForm Computed => _ownForm ? throw new NotImplementedException() : Live(_json);

    internal HeldForm OwnForm => new(_json, ownForm: true);

    internal static HeldForm Live(AppJson json) => new(json, ownForm: false);

    internal LitExpression Beside<TValue>(TValue value) => _ownForm ? throw new NotImplementedException() : Literals.Of(value, _json);

    internal Expression IntoLive(Expression held) => held;

    internal bool WritesAsAScalar<TValue>() => _ownForm ? throw new NotImplementedException() : _json.LiveForm.GetTypeInfo(typeof(TValue)).Kind == JsonTypeInfoKind.None;
}
