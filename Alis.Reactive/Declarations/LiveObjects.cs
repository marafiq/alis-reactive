using System;
using System.Collections.Generic;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// An array of objects a property holds or a method returns, each read through its declaration.
/// </summary>
/// <remarks>
/// A lambda takes the item's declaration and returns one of its members or a comparison of them, so an item's
/// properties and methods are read where the operation runs. A list sums or sorts through <c>Select</c>, whose
/// result is a list of values.
/// </remarks>
/// <typeparam name="TItem">The declaration of each object the array holds.</typeparam>
public sealed class LiveObjects<TItem>
    where TItem : JsObject, new()
{
    internal LiveObjects()
    {
    }

    /// <summary>Reaches the item at a position.</summary>
    /// <param name="position">The item's position, the first at zero.</param>
    /// <returns>The item, whose members are read, set and called through its declaration.</returns>
    public TItem At(int position) => throw new NotImplementedException();

    /// <summary>Counts the list's items.</summary>
    /// <returns>A source of the number of items.</returns>
    public TypedSource<int> Count() => throw new NotImplementedException();

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>The list of the items kept, in the list's order.</returns>
    public LiveObjects<TItem> Where(Func<TItem, TypedSource<bool>> keep) => throw new NotImplementedException();

    /// <summary>Reads one member of every item.</summary>
    /// <param name="field">The member read from each item.</param>
    /// <returns>A source of a new list of the member's values, in the list's order.</returns>
    public TypedSource<IReadOnlyList<TResult>> Select<TResult>(Func<TItem, TypedSource<TResult>> field) => throw new NotImplementedException();

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public TypedSource<int> Count(Func<TItem, TypedSource<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Tests that the list holds an item.</summary>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public TypedSource<bool> Any() => throw new NotImplementedException();

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public TypedSource<bool> Any(Func<TItem, TypedSource<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public TypedSource<bool> All(Func<TItem, TypedSource<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <remarks>
    /// Reading, setting or calling a member of the item stops the pipeline when no item matched. To read one
    /// member of an item that may not be there, give the member to <c>Find</c> as its second argument.
    /// </remarks>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>The item, whose members are read, set and called through its declaration.</returns>
    public TItem Find(Func<TItem, TypedSource<bool>> predicate) => throw new NotImplementedException();

    /// <summary>Finds the first item a condition holds for and reads one member of it.</summary>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The member read from the item found.</param>
    /// <returns>A source of the member's value, which cannot be read when no item matched.</returns>
    public TypedSource<TResult> Find<TResult>(Func<TItem, TypedSource<bool>> predicate, Func<TItem, TypedSource<TResult>> field) => throw new NotImplementedException();
}
