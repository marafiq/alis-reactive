using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.TypeKnowledge;

internal static class WrittenMembers
{
    internal static JsonPropertyInfo Of(MemberExpression access, Type holder, LambdaExpression read, JsonSerializerOptions options)
    {
        var declared = Declared(access, holder, read, options);
        return options.GetTypeInfo(holder).Properties.FirstOrDefault(property => property.Get is not null && property.Name == declared.Name)
            ?? throw NotWritten(read, holder);
    }

    private static JsonPropertyInfo Declared(MemberExpression access, Type holder, LambdaExpression read, JsonSerializerOptions options)
    {
        var declaring = DeclaringClass(access) ?? throw NotWritten(read, holder);
        return options.GetTypeInfo(declaring).Properties.FirstOrDefault(property => Equals(property.AttributeProvider, access.Member))
            ?? throw NotWritten(read, holder);
    }

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-06: the class that declares a lambda's member is a type's identity, not a discovery of members (D18.1, D18.3).")]
    private static Type? DeclaringClass(MemberExpression access) => access.Member.DeclaringType;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "A type's name is its identity (D18.1), and X18.8's refusal names the class a read starts from by it.")]
    private static string NameOf(Type holder) => holder.Name;

    private static InvalidOperationException NotWritten(LambdaExpression read, Type holder)
        => new($"'{read}' reads a member of '{NameOf(holder)}' that the app's JSON options do not write, so the browser never holds it.");
}
