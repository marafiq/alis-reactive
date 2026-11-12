using System;
using System.Linq.Expressions;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// What a declared method returned at the line that called it: a source of that value.
/// </summary>
/// <remarks>
/// The call runs once, where the pipeline reaches its line, and every read of the result reads what it returned
/// then, so a later step that changes the object leaves the result as it was.
/// </remarks>
/// <typeparam name="TResult">The type the object's API gives the method's result.</typeparam>
public sealed class MethodResult<TResult> : TypedSource<TResult>
{
    internal MethodResult()
    {
    }

    /// <summary>Reads data past the result: a field of the value the method returned, at any depth.</summary>
    /// <remarks>
    /// A field the value lacks reads as a new instance of its holder's type would hold it, and a field past an
    /// object that is null cannot be read.
    /// </remarks>
    /// <param name="path">The field to read, as a member path from the result.</param>
    /// <returns>A source of the field's value.</returns>
    public TypedSource<TRead> Read<TRead>(Expression<Func<TResult, TRead>> path) => throw new NotImplementedException();
}
