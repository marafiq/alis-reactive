using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class HeldForm
{
    private readonly AppJson _json;
    private readonly JsonSerializerOptions _baseOptions;
    private readonly bool _fromApp;

    private HeldForm(AppJson json, CultureInfo rendersUnder, JsonSerializerOptions baseOptions, bool fromApp, OwnWriting? own)
    {
        _json = json;
        _baseOptions = baseOptions;
        _fromApp = fromApp;
        RendersUnder = rendersUnder;
        Own = own;
    }

    internal CultureInfo RendersUnder { get; }

    internal OwnWriting? Own { get; }

    internal HeldForm Computed => Live(_json, RendersUnder);

    internal HeldForm Base => Own is null ? this : new(_json, RendersUnder, _baseOptions, _fromApp, null);

    internal string BaseFormName => _fromApp ? "the app's JSON" : "JavaScript's value";

    internal JsonSerializerOptions DataOptions => Own is { } own ? own.Member.OwnForm() : _baseOptions;

    internal bool HoldsAListItsConverterWrites
        => Own is { Member: { HasItsOwnConverter: true } member } && member.Contract.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary;

    internal static HeldForm Live(AppJson json, CultureInfo rendersUnder) => new(json, rendersUnder, json.LiveForm, false, own: null);

    internal static HeldForm App(AppJson json, CultureInfo rendersUnder) => new(json, rendersUnder, json.App, true, null);

    internal HeldForm WrittenBy(WrittenMember member, string read) => new(_json, RendersUnder, _baseOptions, _fromApp, new OwnWriting(member, read));

    internal OwnWriting? WritesOtherwiseThan(HeldForm other)
    {
        var alike = Own is { } own ? other.Own is { } others && own.Member.WritesAlike(others.Member) : other.Own is null;
        return alike ? null : Own ?? other.Own;
    }

    internal LitExpression Beside<TValue>(TValue value) => Own is { } own ? new(own.Member.Written(value, typeof(TValue))) : new(JsonSerializer.SerializeToElement(value, DataOptions));

    internal LitExpression Beside(object? value, Type type) => Own is { } own ? new(own.Member.Written(value, type)) : new(JsonSerializer.SerializeToElement(value, DataOptions.GetTypeInfo(type)));

    internal Expression IntoLive<TValue>(Expression held)
    {
        if (Own is not { } own)
            return _fromApp && LiveShapes.Converts(DataOptions.GetTypeInfo(typeof(TValue)), out var appShape) ? new AsExpression(held, appShape) : held;
        if (own.Member.HasItsOwnConverter)
            throw new InvalidOperationException($"A live object takes '{own.Read}' as JavaScript holds its type, and its own JSON converter writes it otherwise; pass the value from the server.");
        return LiveShapes.Converts(own.Member.OwnForm().GetTypeInfo(typeof(TValue)), out var shape) ? new AsExpression(held, shape) : held;
    }

    internal void RefuseAComputation()
    {
        if (Own is not { } own) return;
        throw new InvalidOperationException($"'{own.Read}' is written by {own.Cause}, unlike {BaseFormName}, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.");
    }

    internal bool WritesOwnScalar(Type type)
    {
        var scalar = Nullable.GetUnderlyingType(type) ?? type;
        return Own is not null || (_fromApp && (DataOptions.Converters.Any(converter => converter.CanConvert(type)) ||
            !ReferenceEquals(DataOptions.GetTypeInfo(scalar).Converter, PlanJson.Options.GetTypeInfo(scalar).Converter)));
    }

    internal bool WritesAsAScalar<TValue>() => WritesAsAScalar(typeof(TValue));

    internal bool WritesAsAScalar(Type type) => DataOptions.GetTypeInfo(type).Kind == JsonTypeInfoKind.None;

    internal sealed record OwnWriting(WrittenMember Member, string Read)
    {
        internal string Cause => Member.WhatWritesItsOwnForm;
    }
}
