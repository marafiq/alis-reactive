using System;
using System.Linq.Expressions;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// A member of a row, item or card class, named for a component whose method takes a member's name.
/// </summary>
/// <remarks>
/// A declared method that takes a field declares its argument as <c>Arg&lt;Field&gt;</c>, and a view passes
/// <c>Field.Of((Resident r) =&gt; r.CareLevel)</c>, so the compiler checks the member. The component receives the
/// member's name as the app's JSON names it, the name the app's data holds.
/// </remarks>
public abstract class Field : IArgumentValue
{
    private readonly LambdaExpression _member;

    private protected Field(LambdaExpression member)
    {
        _member = member;
    }

    Alis.Reactive.Schema.Expression IArgumentValue.Written(AppJson json)
        => Literals.Text(MemberPath.NameOfField(_member, json.LiveForm));

    /// <summary>Names the member a lambda selects; the member's type travels with the field.</summary>
    /// <typeparam name="TRow">The class that holds the member.</typeparam>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <param name="member">The member, as <c>(Resident r) =&gt; r.CareLevel</c>.</param>
    /// <returns>The field.</returns>
    public static Field<TValue> Of<TRow, TValue>(Expression<Func<TRow, TValue>> member) => new(member);
}

/// <summary>A <see cref="Field"/> whose member holds a <typeparamref name="TValue"/>.</summary>
/// <typeparam name="TValue">The type of the member.</typeparam>
public sealed class Field<TValue> : Field
{
    internal Field(LambdaExpression member) : base(member)
    {
    }
}
