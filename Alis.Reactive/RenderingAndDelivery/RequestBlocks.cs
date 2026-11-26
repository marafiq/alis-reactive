using System;
using System.IO;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;
using Alis.Reactive.Validation;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// An element a view renders as a request container: the block that holds the container's markup.
/// </summary>
/// <remarks>
/// The block renders the container's element and its one summary, and ends the element where the block ends. A
/// request sends the container's inputs by <see cref="Container"/>, each under its name past the field prefix the view
/// was rendered under, so an endpoint that binds <typeparamref name="TModel"/> binds them; an input inside the element
/// that the prefix does not open stops the request. The validator registered for
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
        _block = new RequestBlock(writer, plan.Writer.Content, id, plan.StartPrefix, LiveChecks.Of<TModel>(plan.Writer.Content, id), checkPrefix);
        Container = new RequestContainer<TModel>(id, plan.Writer.Content);
    }

    /// <summary>Gets the container's handle, which a request sends and a step validates.</summary>
    public RequestContainer<TModel> Container { get; }

    /// <summary>Ends the block: closes the container's element.</summary>
    public void Dispose() => _block.Close();
}
