using System;
using System.Collections.Generic;
using Alis.Reactive.Lists;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public sealed class LiveObjects<TItem>
    where TItem : JsObject, new()
{
    private readonly Expression _list;
    private readonly PlanBuild _build;
    private readonly ReadPresence _members;

    internal LiveObjects(Expression list, PlanBuild build, ReadPresence members)
    {
        _list = list;
        _build = build;
        _members = members;
    }

    public TItem At(int position)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "An item's position is zero or more.");
        return JsObject.Bound<TItem>(new Binding(new GetExpression(_list, [PathStep.Index(position)], _members), _build, _members));
    }

    public TypedSource<int> Count() => new DataRead<int>(ListForms.Length(_list), _build);

    public LiveObjects<TItem> Where(Func<TItem, TypedSource<bool>> keep)
        => new(ListForms.Filter(_list, ItemRead(keep)), _build, _members);

    public TypedSource<IReadOnlyList<TResult>> Select<TResult>(Func<TItem, TypedSource<TResult>> field)
        => new DataRead<IReadOnlyList<TResult>>(ListForms.Map(_list, ItemRead(field)), _build);

    public TypedSource<int> Count(Func<TItem, TypedSource<bool>> predicate)
        => new DataRead<int>(ListForms.Length(ListForms.Filter(_list, ItemRead(predicate))), _build);

    public TypedSource<bool> Any() => new DataRead<bool>(ListForms.Some(_list, ListForms.EveryItem(_build.ItemName())), _build);

    public TypedSource<bool> Any(Func<TItem, TypedSource<bool>> predicate)
        => new DataRead<bool>(ListForms.Some(_list, ItemRead(predicate)), _build);

    public TypedSource<bool> All(Func<TItem, TypedSource<bool>> predicate)
        => new DataRead<bool>(ListForms.Every(_list, ItemRead(predicate)), _build);

    public TItem Find(Func<TItem, TypedSource<bool>> predicate)
        => JsObject.Bound<TItem>(new Binding(ListForms.Find(_list, ItemRead(predicate)), _build, ReadPresence.Required));

    public TypedSource<TResult> Find<TResult>(Func<TItem, TypedSource<bool>> predicate, Func<TItem, TypedSource<TResult>> field)
    {
        var found = JsObject.Bound<TItem>(new Binding(ListForms.Find(_list, ItemRead(predicate)), _build, ReadPresence.RequiredIfHeld));
        var source = _build.ReadingItems(() => field(found))
            ?? throw new ArgumentException("A list's lambda reads a member of its item, such as bed => bed.Resident.", nameof(field));
        return new DataRead<TResult>(source.Expression, _build);
    }

    private FnExpression ItemRead<TResult>(Func<TItem, TypedSource<TResult>> read)
    {
        var name = _build.ItemName();
        var item = JsObject.Bound<TItem>(new Binding(new RefExpression(name), _build, ReadPresence.Required));
        var source = _build.ReadingItems(() => read(item))
            ?? throw new ArgumentException("A list's lambda reads a member of its item, such as bed => bed.Resident.", nameof(read));
        return new FnExpression([name], source.Expression);
    }
}
