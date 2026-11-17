using System;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// Plain data a handler receives, such as the payload of an app event: a source of the whole value whose members
/// are read with <c>Read</c>.
/// </summary>
/// <remarks>
/// A member the data lacks reads the value the app's JSON gives it in a new object of its class, and data that
/// holds nothing reads nothing. A number, a boolean, an enum written as its number and a date read as their type,
/// whether the data holds them as JSON writes them or as text.
/// </remarks>
/// <typeparam name="T">The class of the data, as the app's JSON writes it.</typeparam>
public sealed class Data<T> : TypedSource<T>
{
    private readonly BlockWriter _writer;

    internal Data(BlockWriter writer)
        : this(writer, HeldForm.Live(writer.Content.Json, writer.Content.RendersUnder))
    {
    }

    internal Data(BlockWriter writer, HeldForm form)
    {
        _writer = writer;
        Expression = new Received(writer).Target;
        Form = form;
    }

    internal override Schema.Expression Expression { get; }

    internal override HeldForm Form { get; }

    /// <summary>Reads a member of the data, as <c>x =&gt; x.Pager.Number</c>.</summary>
    /// <typeparam name="TResult">The type of the member.</typeparam>
    /// <param name="path">The member, as a member path.</param>
    /// <returns>A source of the member's value.</returns>
    public TypedSource<TResult> Read<TResult>(Expression<Func<T, TResult>> path)
    {
        var live = _writer.Content.Json.LiveForm;
        var read = DataReads.Past<TResult>(Expression, Form, path, live);
        var writtenByItsOwnConverter = read.Form.Own is { Member.HasItsOwnConverter: true };
        if (writtenByItsOwnConverter) return read;
        var member = live.GetTypeInfo(typeof(TResult));
        var shape = PayloadShapes.ReadAs(member) ?? PayloadShapes.WrittenAsANumber(JsonSerializer.SerializeToElement(default(TResult), member), member);
        return shape is null ? read : new Computed<TResult>(new AsExpression(read.Expression, shape), Form);
    }
}
