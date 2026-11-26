using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.TypeKnowledge;

internal sealed class WrittenMember
{
    private const JsonNumberHandling WritingBits = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowNamedFloatingPointLiterals;

    private static readonly Type[] Numbers =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
        typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?),
        typeof(float?), typeof(double?), typeof(decimal?),
    ];

    private readonly JsonPropertyInfo _property;
    private readonly Func<object, object?> _get;

    private WrittenMember(JsonTypeInfo holder, JsonPropertyInfo property, Func<object, object?> get)
    {
        Holder = holder;
        _property = property;
        _get = get;
    }

    internal JsonTypeInfo Holder { get; }

    internal string Name => _property.Name;

    internal JsonTypeInfo Contract => Holder.Options.GetTypeInfo(_property.PropertyType);

    internal bool HasItsOwnConverter => _property.CustomConverter is not null;

    internal bool HoldsItsOwnForm => !ReferenceEquals(OwnForm(), Holder.Options);

    internal string WhatWritesItsOwnForm => HasItsOwnConverter
        ? "its own JSON converter"
        : _property.NumberHandling is null ? "its class's JSON number handling" : "its own JSON number handling";

    internal object? ReadFrom(object holder) => _get(holder);

    internal bool WritesAlike(WrittenMember other)
    {
        var sameMember = ReferenceEquals(Holder.Options, other.Holder.Options) && Holder.Type == other.Holder.Type
            && Name == other.Name && Equals(_property.AttributeProvider, other._property.AttributeProvider);
        if (sameMember) return true;
        var bothByNumberHandling = !HasItsOwnConverter && !other.HasItsOwnConverter;
        return bothByNumberHandling && (Handling & WritingBits) == (other.Handling & WritingBits);
    }

    internal JsonElement Written(object? value, Type type)
        => JsonSerializer.SerializeToElement(value, OwnForm().GetTypeInfo(HasItsOwnConverter ? _property.PropertyType : type));

    internal static WrittenMember Of(MemberExpression access, JsonTypeInfo holder, LambdaExpression read)
    {
        var declared = Declared(access, holder, read);
        var written = holder.Properties.FirstOrDefault(property => property.Get is not null && property.Name == declared.Name);
        return written is { Get: { } get } ? new WrittenMember(holder, written, get) : throw NotWritten(read, holder);
    }

    internal static bool LeftOutByItsClass(MemberExpression access, JsonSerializerOptions options)
    {
        if (DeclaringClass(access) is not { } declaring) return false;
        var declared = options.GetTypeInfo(declaring).Properties.FirstOrDefault(property => Equals(property.AttributeProvider, access.Member));
        return declared is not { Get: not null };
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-16: a member the app's JSON options leave out is named as C# declares it, the name the browser holds its start under (D16.6, X18.6).")]
    internal static string AsCSharpDeclaresIt(MemberExpression access) => access.Member.Name;

    internal static void Carry(JsonTypeInfo holder, MemberExpression access, Func<object, object?> read)
    {
        foreach (var ignored in holder.Properties.Where(property => Equals(property.AttributeProvider, access.Member)).ToList()) holder.Properties.Remove(ignored);
        var carried = holder.CreateJsonPropertyInfo(access.Type, AsCSharpDeclaresIt(access));
        carried.Get = read;
        carried.AttributeProvider = access.Member;
        holder.Properties.Add(carried);
    }

    internal JsonSerializerOptions OwnForm()
    {
        var options = Holder.Options;
        if (_property.CustomConverter is { } converter)
        {
            var byItsConverter = new JsonSerializerOptions(options);
            byItsConverter.Converters.Insert(0, converter);
            return byItsConverter;
        }

        var writesAsTheOptionsDo = (Handling & WritingBits) == (options.NumberHandling & WritingBits) || !HoldsNumbers(Contract);
        return writesAsTheOptionsDo ? options : new JsonSerializerOptions(options) { NumberHandling = Handling };
    }

    private JsonNumberHandling Handling => _property.NumberHandling ?? Holder.NumberHandling ?? Holder.Options.NumberHandling;

    private static bool HoldsNumbers(JsonTypeInfo type)
    {
        if (Numbers.Contains(type.Type)) return true;
        if (type.IsList(out var item)) return Numbers.Contains(item.Type);
        return type.IsDictionary(out _, out var value) && Numbers.Contains(value.Type);
    }

    private static JsonPropertyInfo Declared(MemberExpression access, JsonTypeInfo holder, LambdaExpression read)
    {
        var declaring = DeclaringClass(access) ?? throw NotWritten(read, holder);
        return holder.Options.GetTypeInfo(declaring).Properties.FirstOrDefault(property => Equals(property.AttributeProvider, access.Member))
            ?? throw NotWritten(read, holder);
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: the class that declares a lambda's member is a type's identity, not a discovery of members (D18.1, D18.3).")]
    internal static Type? DeclaringClass(MemberExpression access) => access.Member.DeclaringType;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-16: a contract's member names no C# member, so a refusal names it by the member behind it (D18.1, M1.X8, M1.X9).")]
    internal static string AsCSharpDeclaresIt(JsonTypeInfo holder, JsonPropertyInfo member)
        => member.AttributeProvider is MemberInfo declared
            ? declared.Name
            : throw new InvalidOperationException($"'{ClassNames.AsCSharpWritesIt(holder.Type)}' holds '{member.Name}', a member the app's JSON contract adds and no C# member declares, so the core cannot name it as C# declares it.");

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's list names this file's read of a member's C# name (D18.1): MVC 5 binds a request body by it, so the browser's check names each member it reads by it, and a member no C# member declares keeps the contract's name (D2.4, D16.6).")]
    internal static string AsCSharpDeclaresItElseAsWritten(JsonPropertyInfo member)
        => member.AttributeProvider is MemberInfo declared ? declared.Name : member.Name;

    private static InvalidOperationException NotWritten(LambdaExpression read, JsonTypeInfo holder)
        => new($"'{read}' reads a member of '{ClassNames.AsCSharpWritesIt(holder.Type)}' that the app's JSON options do not write, so the browser never holds it.");
}
