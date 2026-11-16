using System;
using System.IO;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.Validation;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// A native form a view renders as a request container: the block that holds the form's markup.
/// </summary>
/// <remarks>
/// The block renders the form's element and its one summary, and ends the element where the block ends, as
/// <c>using (Html.BeginForm())</c> does. A request sends the form by <see cref="Form"/>. The validator registered for
/// <typeparamref name="TModel"/> checks the form's inputs as a person works: typing clears an input's message, and
/// leaving the input or committing its value checks its member over what the form sends, showing the first failure at
/// the input.
/// </remarks>
/// <typeparam name="TModel">The model of the view that renders the form.</typeparam>
public sealed class RequestFormBlock<TModel> : IDisposable
{
    private readonly RequestBlock _block;

    internal RequestFormBlock(TextWriter writer, ReactivePlan<TModel> plan, string id, Action checkPrefix)
    {
        _block = new RequestBlock(writer, plan.Writer.Content, id, ContainerKind.Form, LiveChecks.Of<TModel>(plan.Writer.Content, id), checkPrefix);
        Form = new RequestForm<TModel>(id, plan.Writer.Content);
    }

    /// <summary>Gets the form's handle, which a request sends and a step validates.</summary>
    public RequestForm<TModel> Form { get; }

    /// <summary>Ends the block: closes the form's element.</summary>
    public void Dispose() => _block.Close();
}

/// <summary>
/// An element a view renders as a request container: the block that holds the container's markup.
/// </summary>
/// <remarks>
/// The block renders the container's element and its one summary, and ends the element where the block ends. A
/// request sends the container's inputs by <see cref="Container"/>. The validator registered for
/// <typeparamref name="TModel"/> checks the container's inputs as a person works: typing clears an input's message,
/// and leaving the input or committing its value checks its member over what the container sends, showing the first
/// failure at the input.
/// </remarks>
/// <typeparam name="TModel">The model of the view that renders the container.</typeparam>
public sealed class RequestContainerBlock<TModel> : IDisposable
{
    private readonly RequestBlock _block;

    internal RequestContainerBlock(TextWriter writer, ReactivePlan<TModel> plan, string id, Action checkPrefix)
    {
        _block = new RequestBlock(writer, plan.Writer.Content, id, ContainerKind.Element, LiveChecks.Of<TModel>(plan.Writer.Content, id), checkPrefix);
        Container = new RequestContainer<TModel>(id, plan.Writer.Content);
    }

    /// <summary>Gets the container's handle, which a request sends and a step validates.</summary>
    public RequestContainer<TModel> Container { get; }

    /// <summary>Ends the block: closes the container's element.</summary>
    public void Dispose() => _block.Close();
}
