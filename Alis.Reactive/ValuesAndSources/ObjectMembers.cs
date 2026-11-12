using System;
using System.Linq.Expressions;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// The members of an object a step builds when it runs: each a literal or what a source reads then.
/// </summary>
/// <typeparam name="T">The class of the object built.</typeparam>
public sealed class ObjectMembers<T>
{
    internal ObjectMembers()
    {
    }

    /// <summary>Gives a member of the object what a source reads when the step runs.</summary>
    /// <remarks>A source that cannot be read gives the member null.</remarks>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The source of the member's value.</param>
    /// <typeparam name="TValue">The member's type.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue>> member, ITypedSource<TValue> value) => throw new NotImplementedException();

    /// <summary>Gives a member of the object a literal.</summary>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The member's value, written into the plan where the view renders.</param>
    /// <typeparam name="TValue">The member's type.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue>> member, TValue value) => throw new NotImplementedException();

    /// <summary>Gives a member of a nullable value type what a source of the type itself reads when the step runs.</summary>
    /// <param name="member">The member of the object, as a member path.</param>
    /// <param name="value">The source of the member's value.</param>
    /// <typeparam name="TValue">The value type the member holds or null.</typeparam>
    /// <returns>The object's members.</returns>
    public ObjectMembers<T> Set<TValue>(Expression<Func<T, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotImplementedException();
}
