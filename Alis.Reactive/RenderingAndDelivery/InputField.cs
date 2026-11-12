using System;
using System.Linq.Expressions;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// An input bound to a member of the view's model, for a component's render helper to render: its id, the name it
/// binds under and the value it starts with.
/// </summary>
/// <typeparam name="TModel">The model of the view that renders the input.</typeparam>
/// <typeparam name="TValue">The type of the member the input binds.</typeparam>
public sealed partial class InputField<TModel, TValue>
{
    internal InputField()
    {
    }

    /// <summary>Gets the id the framework gives the input's element: the class that declares the member, then the member's path.</summary>
    public string Id => throw new NotImplementedException();

    /// <summary>Gets the name the input binds under: the member's full field name in its view.</summary>
    public string Name => throw new NotImplementedException();

    /// <summary>Gets the member the input binds.</summary>
    public Expression<Func<TModel, TValue>> Member => throw new NotImplementedException();

    /// <summary>Gets the member's value as the plan writes a literal: the state the component starts in.</summary>
    /// <remarks>It is <c>null</c> when an object on the way to the member holds nothing.</remarks>
    public string ValueJson => throw new NotImplementedException();

    /// <summary>Writes the options a render helper starts its component with, as the plan writes a literal.</summary>
    /// <param name="options">The options to write.</param>
    /// <returns>The options' JSON.</returns>
    public string Json<TOptions>(TOptions options) => throw new NotImplementedException();
}
