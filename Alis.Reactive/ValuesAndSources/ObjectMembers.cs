using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Expression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// The members of an object a step builds when it runs: each a literal or what a source reads then.
/// </summary>
/// <typeparam name="T">The class of the object built.</typeparam>
public sealed class ObjectMembers<T>
{
    private readonly HeldForm _form;
    private readonly List<Field> _fields = [];

    internal ObjectMembers(HeldForm form)
    {
        if (form.DataOptions.GetTypeInfo(typeof(T)).Kind != JsonTypeInfoKind.Object)
        {
            throw new InvalidOperationException(
                "Object<T> builds named members of a JSON object; pass a scalar, a list or a value written by a converter as a literal or a source of its own type.");
        }

        _form = form;
    }

    /// <summary>Gives a member of the object what a source reads when the step runs.</summary>
    /// <remarks>A source that reads nothing gives the member null. A read that stops the pipeline stops it here.</remarks>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The source of the member's value.</param>
    /// <typeparam name="TValue">The member's type.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue>> member, ITypedSource<TValue> value)
        => SetSource(member, value);

    /// <summary>Gives a member of the object a literal.</summary>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The member's value, written into the plan where the view renders.</param>
    /// <typeparam name="TValue">The member's type.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue>> member, TValue value)
    {
        var path = Path(member);
        var hasOwnForm = path.Member.HoldsItsOwnForm;
        if (!hasOwnForm) RefuseAnInexactNumber(value, member.Body.ToString());
        var written = path.Member.Written(value, typeof(TValue));
        if (hasOwnForm && written.ValueKind == JsonValueKind.Number) RefuseAnInexactNumber(value, member.Body.ToString());
        return Add(path.WireNames, new LitExpression(written), member);
    }

    /// <summary>Gives a member of a nullable value type what a source of the type itself reads when the step runs.</summary>
    /// <remarks>A source that reads nothing gives the member null. A read that stops the pipeline stops it here.</remarks>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The source of the member's value.</param>
    /// <typeparam name="TValue">The value type the member holds or null.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => SetSource(member, value);

    internal RecordExpression Written() => Record(_fields, 0);

    private ObjectMembers<T> SetSource<TValue>(LambdaExpression member, ITypedSource<TValue> value)
    {
        var path = Path(member);
        var sourceForm = Sources.FormOf(value);
        var expression = Sources.Of(value);
        if (!path.Member.HoldsItsOwnForm) return Add(path.WireNames, sourceForm.IntoLive<TValue>(expression), member);
        var targetForm = _form.WrittenBy(path.Member, member.Body.ToString());
        if (sourceForm.WritesOtherwiseThan(targetForm) is not null)
        {
            throw new InvalidOperationException(
                $"Object member '{member.Body}' is written by {path.Member.WhatWritesItsOwnForm}, unlike the source; pass the member's value from the server, or a source in the member's own form.");
        }

        return Add(path.WireNames, expression, member);
    }

    private MemberPath Path(LambdaExpression member)
    {
        MemberPath? path;
        try
        {
            if (MemberPath.TryOf(member, _form.DataOptions, out path)) path.RefuseAReadPastItsOwnConverter(member.Body.ToString());
        }
        catch (InvalidOperationException error)
        {
            throw new InvalidOperationException(
                $"Object member '{member.Body}' cannot be set: {error.Message} Set a member the app's JSON contract writes, or pass the containing object as a value from the server.", error);
        }

        return path ?? throw new InvalidOperationException(
            $"'{member}' names no object member; set a member and the members past it, such as order => order.Destination.Ward.");
    }

    private ObjectMembers<T> Add(IReadOnlyList<string> path, Expression value, LambdaExpression member)
    {
        if (_fields.Any(field => field.Path.Take(Math.Min(field.Path.Count, path.Count)).SequenceEqual(path.Take(Math.Min(field.Path.Count, path.Count)), StringComparer.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Object member '{member.Body}' overlaps a member already supplied; set each member once, and set either the containing object or its members.");
        }

        _fields.Add(new Field(path, value));
        return this;
    }

    private static RecordExpression Record(IEnumerable<Field> fields, int depth)
        => new([.. fields.GroupBy(field => field.Path[depth], StringComparer.Ordinal).Select(group =>
            new RecordField(group.Key, group.First().Path.Count == depth + 1 ? group.First().Value : Record(group, depth + 1)))]);

    private static void RefuseAnInexactNumber<TValue>(TValue value, string member)
    {
        var reason = value switch
        {
            double number when double.IsNaN(number) || double.IsInfinity(number) => "NaN or an infinity, which no JSON number holds",
            float number when float.IsNaN(number) || float.IsInfinity(number) => "NaN or an infinity, which no JSON number holds",
            decimal number when number.ToString(CultureInfo.InvariantCulture).Replace(".", "").TrimStart('-', '0').TrimEnd('0').Length > 15
                => "a decimal of more than 15 significant digits",
            long number when number is < -9007199254740991L or > 9007199254740991L => "a whole number past 9007199254740991",
            ulong number when number > 9007199254740991UL => "a whole number past 9007199254740991",
            _ => string.Empty,
        };
        if (reason.Length == 0) return;
        throw new InvalidOperationException($"'{member}' holds a JavaScript number, and {Convert.ToString(value, CultureInfo.InvariantCulture)} is {reason}, which JavaScript reads as another value; write a finite number of at most 15 significant digits.");
    }

    private sealed record Field(IReadOnlyList<string> Path, Expression Value);
}
