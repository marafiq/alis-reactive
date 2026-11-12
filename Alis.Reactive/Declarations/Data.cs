using System;
using System.Linq.Expressions;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// Plain data a handler receives, such as the payload of an app event: a source of the whole value whose members
/// are read with <c>Read</c>.
/// </summary>
/// <remarks>
/// A member the data lacks reads the value the app's JSON gives it in a new object of its class, and data that
/// holds nothing reads nothing.
/// </remarks>
/// <typeparam name="T">The class of the data, as the app's JSON writes it.</typeparam>
public sealed class Data<T> : TypedSource<T>
{
    internal Data()
    {
    }

    /// <summary>Reads a member of the data, as <c>x =&gt; x.Pager.Number</c>.</summary>
    /// <typeparam name="TResult">The type of the member.</typeparam>
    /// <param name="path">The member, as a member path.</param>
    /// <returns>A source of the member's value.</returns>
    public TypedSource<TResult> Read<TResult>(Expression<Func<T, TResult>> path) => throw new NotImplementedException();
}
