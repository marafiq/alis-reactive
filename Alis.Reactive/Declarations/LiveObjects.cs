using System;
using System.Collections.Generic;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Schema;
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
    private readonly Binding _bound;
    private readonly bool _itemsMayBeAbsent;

    internal LiveObjects(Binding bound)
        : this(bound, false)
    {
    }

    internal LiveObjects(Binding bound, bool itemsMayBeAbsent)
    {
        _bound = bound;
        _itemsMayBeAbsent = itemsMayBeAbsent;
    }

    internal Schema.Expression Target => _bound.Target;

    internal HeldForm Form => HeldForm.Live(_bound.Writer.Content.Json, _bound.Writer.Content.RendersUnder);

    /// <summary>Reaches the item at a position.</summary>
    /// <param name="position">The item's position, the first at zero.</param>
    /// <returns>The item, whose members are read, set and called through its declaration.</returns>
    public TItem At(int position)
    {
        if (position < 0) throw new InvalidOperationException("An item's position is zero or more.");
        return JsObject.Reaching<TItem>(new Reached(() => new GetExpression([new IndexStep(position)], PathMembers.RequiredIfHeld, _bound.Target), _bound.Writer, _itemsMayBeAbsent));
    }

    /// <summary>Counts the list's items.</summary>
    /// <returns>A source of the number of items.</returns>
    public TypedSource<int> Count() => new Computed<int>(ListForms.Length(_bound.Target), Form);

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>The list of the items kept, in the list's order.</returns>
    public LiveObjects<TItem> Where(Func<TItem, TypedSource<bool>> keep)
        => new(new Reached(ListForms.Filter(_bound.Target, Lambda(keep).Function), _bound.Writer), _itemsMayBeAbsent);

    /// <summary>Reads one member of every item.</summary>
    /// <param name="field">The member read from each item.</param>
    /// <returns>A source of a new list of the member's values, in the list's order.</returns>
    public TypedSource<IReadOnlyList<TResult>> Select<TResult>(Func<TItem, TypedSource<TResult>> field)
    {
        var list = _bound.Target;
        var selected = Lambda(field);
        var function = default(TResult) is null
            ? selected.Function
            : new FnExpression(selected.Function.Params, new OpExpression(Operator.NonNull, [selected.Function.Body]));
        return new Computed<IReadOnlyList<TResult>>(ListForms.Map(list, function), selected.Form);
    }

    /// <summary>Reads the object every item holds.</summary>
    /// <param name="held">The object read from each item, a member of the item that holds one.</param>
    /// <typeparam name="TObject">The declaration of the object each item holds.</typeparam>
    /// <returns>A new list of the objects, in the list's order, each read through its declaration.</returns>
    public LiveObjects<TObject> Select<TObject>(Func<TItem, TObject> held)
        where TObject : JsObject, new()
    {
        var list = _bound.Target;
        var selected = OverEachItem(item =>
        {
            var reached = held(item).Bound;
            return (reached.Target, reached.MayBeAbsent);
        });
        return new(new Reached(ListForms.Map(list, selected.Function), _bound.Writer), selected.Value);
    }

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public TypedSource<int> Count(Func<TItem, TypedSource<bool>> predicate)
        => new Computed<int>(ListForms.Length(ListForms.Filter(_bound.Target, Lambda(predicate).Function)), Form);

    /// <summary>Tests that the list holds an item.</summary>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public TypedSource<bool> Any()
        => new Computed<bool>(ListForms.AnyOf(_bound.Target, new FnExpression([_bound.Writer.NextLiveItem()], Literals.Of(true, _bound.Writer.Content.Json))), Form);

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public TypedSource<bool> Any(Func<TItem, TypedSource<bool>> predicate)
        => new Computed<bool>(ListForms.AnyOf(_bound.Target, Lambda(predicate).Function), Form);

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public TypedSource<bool> All(Func<TItem, TypedSource<bool>> predicate)
        => new Computed<bool>(ListForms.AllOf(_bound.Target, Lambda(predicate).Function), Form);

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <remarks>
    /// Reading, setting or calling a member of the item stops the pipeline when no item matched. To read one
    /// member of an item that may not be there, give the member to <c>Find</c> as its second argument.
    /// </remarks>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>The item, whose members are read, set and called through its declaration.</returns>
    public TItem Find(Func<TItem, TypedSource<bool>> predicate)
        => Found(predicate, false);

    /// <summary>Finds the first item a condition holds for and reads one member of it.</summary>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The member read from the item found.</param>
    /// <returns>A source of the member's value, which cannot be read when no item matched.</returns>
    public TypedSource<TResult> Find<TResult>(Func<TItem, TypedSource<bool>> predicate, Func<TItem, TypedSource<TResult>> field)
    {
        var item = Found(predicate, true);
        var selected = _bound.Writer.Lambda(() =>
        {
            var source = field(item);
            return (source.Expression, source.Form);
        });
        return new Computed<TResult>(selected.Body, selected.Value);
    }

    private TItem Found(Func<TItem, TypedSource<bool>> predicate, bool mayBeAbsent)
        => JsObject.Reaching<TItem>(new Reached(ListForms.Find(_bound.Target, Lambda(predicate).Function), _bound.Writer, mayBeAbsent || _itemsMayBeAbsent));

    private (FnExpression Function, HeldForm Form) Lambda<TResult>(Func<TItem, TypedSource<TResult>> field)
        => OverEachItem(item =>
        {
            var source = field(item);
            return (source.Expression, source.Form);
        });

    private (FnExpression Function, TValue Value) OverEachItem<TValue>(Func<TItem, (Schema.Expression Body, TValue Value)> read)
    {
        var writer = _bound.Writer;
        var name = writer.NextLiveItem();
        var item = JsObject.Reaching<TItem>(new Reached(new RefExpression(name), writer, _itemsMayBeAbsent));
        var selected = writer.Lambda(() => read(item));
        return (new FnExpression([name], selected.Body), selected.Value);
    }
}
