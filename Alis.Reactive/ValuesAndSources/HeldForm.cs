using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class HeldForm
{
    private readonly AppJson _json;
    private readonly bool _ownForm;

    private HeldForm(AppJson json, CultureInfo rendersUnder, bool ownForm)
    {
        _json = json;
        RendersUnder = rendersUnder;
        _ownForm = ownForm;
    }

    internal CultureInfo RendersUnder { get; }

    internal HeldForm Computed => _ownForm ? throw new NotImplementedException() : Live(_json, RendersUnder);

    internal HeldForm OwnForm => new(_json, RendersUnder, ownForm: true);

    internal static HeldForm Live(AppJson json, CultureInfo rendersUnder) => new(json, rendersUnder, ownForm: false);

    internal LitExpression Beside<TValue>(TValue value) => _ownForm ? throw new NotImplementedException() : Literals.Of(value, _json);

    internal LitExpression Beside(object? value, Type type) => _ownForm ? throw new NotImplementedException() : Literals.Of(value, type, _json);

    internal string BaseFormName => _ownForm ? throw new NotImplementedException() : "JavaScript's value";

    internal JsonSerializerOptions DataOptions => _ownForm ? throw new NotImplementedException() : _json.LiveForm;

    internal Expression IntoLive(Expression held) => held;

    internal bool WritesAsAScalar<TValue>() => WritesAsAScalar(typeof(TValue));

    internal bool WritesAsAScalar(Type type) => _ownForm ? throw new NotImplementedException() : _json.LiveForm.GetTypeInfo(type).Kind == JsonTypeInfoKind.None;
}
