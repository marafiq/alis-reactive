using System;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// A native form a view renders as a request container: the block that holds the form's markup.
/// </summary>
/// <remarks>
/// The block renders the form's element and its one summary, and ends the element where the block ends, as
/// <c>using (Html.BeginForm())</c> does. A request sends the form by <see cref="Form"/>.
/// </remarks>
/// <typeparam name="TModel">The model of the view that renders the form.</typeparam>
public sealed class RequestFormBlock<TModel> : IDisposable
{
    internal RequestFormBlock()
    {
    }

    /// <summary>Gets the form's handle, which a request sends and a step validates.</summary>
    public RequestForm<TModel> Form => throw new NotImplementedException();

    /// <summary>Ends the block: closes the form's element.</summary>
    public void Dispose() => throw new NotImplementedException();
}

/// <summary>
/// An element a view renders as a request container: the block that holds the container's markup.
/// </summary>
/// <remarks>
/// The block renders the container's element and its one summary, and ends the element where the block ends. A
/// request sends the container's inputs by <see cref="Container"/>.
/// </remarks>
/// <typeparam name="TModel">The model of the view that renders the container.</typeparam>
public sealed class RequestContainerBlock<TModel> : IDisposable
{
    internal RequestContainerBlock()
    {
    }

    /// <summary>Gets the container's handle, which a request sends and a step validates.</summary>
    public RequestContainer<TModel> Container => throw new NotImplementedException();

    /// <summary>Ends the block: closes the container's element.</summary>
    public void Dispose() => throw new NotImplementedException();
}
