using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ValuesAndSources;

internal sealed class HeldForm
{
    private readonly AppJson _json;

    private HeldForm(AppJson json, CultureInfo rendersUnder, OwnWriting? own)
    {
        _json = json;
        RendersUnder = rendersUnder;
        Own = own;
    }

    internal CultureInfo RendersUnder { get; }

    internal OwnWriting? Own { get; }

    internal HeldForm Computed => Live(_json, RendersUnder);

    internal HeldForm Base => Own is null ? this : Computed;

    internal string BaseFormName => "JavaScript's value";

    internal JsonSerializerOptions DataOptions => Own is { } own ? own.Member.OwnForm() : _json.LiveForm;

    internal bool HoldsAListItsConverterWrites
        => Own is { Member: { HasItsOwnConverter: true } member } && member.Contract.Kind is JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary;

    internal static HeldForm Live(AppJson json, CultureInfo rendersUnder) => new(json, rendersUnder, own: null);

    internal HeldForm WrittenBy(WrittenMember member, string read) => new(_json, RendersUnder, new OwnWriting(member, read));

    internal OwnWriting? WritesOtherwiseThan(HeldForm other)
    {
        var alike = Own is { } own ? other.Own is { } others && own.Member.WritesAlike(others.Member) : other.Own is null;
        return alike ? null : Own ?? other.Own;
    }

    internal LitExpression Beside<TValue>(TValue value) => Own is { } own ? new(own.Member.Written(value, typeof(TValue))) : Literals.Of(value, _json);

    internal LitExpression Beside(object? value, Type type) => Own is { } own ? new(own.Member.Written(value, type)) : Literals.Of(value, type, _json);

    internal Expression IntoLive<TValue>(Expression held)
    {
        if (Own is not { } own) return held;
        if (own.Member.HasItsOwnConverter)
            throw new InvalidOperationException($"A live object takes '{own.Read}' as JavaScript holds its type, and its own JSON converter writes it otherwise; pass the value from the server.");
        return LiveShapes.Converts(own.Member.OwnForm().GetTypeInfo(typeof(TValue)), out var shape) ? new AsExpression(held, shape) : held;
    }

    internal void RefuseAComputation()
    {
        if (Own is not { } own) return;
        throw new InvalidOperationException($"'{own.Read}' is written by {own.Cause}, unlike {BaseFormName}, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.");
    }

    internal bool WritesAsAScalar<TValue>() => WritesAsAScalar(typeof(TValue));

    internal bool WritesAsAScalar(Type type) => DataOptions.GetTypeInfo(type).Kind == JsonTypeInfoKind.None;

    internal sealed record OwnWriting(WrittenMember Member, string Read)
    {
        internal string Cause => Member.WhatWritesItsOwnForm;
    }
}
