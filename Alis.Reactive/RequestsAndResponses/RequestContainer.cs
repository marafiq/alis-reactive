namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// A request container of a view: the element whose inputs a request sends and whose summary shows that request's failures.
/// </summary>
/// <typeparam name="TModel">The model the container's block was rendered for.</typeparam>
public class RequestContainer<TModel>
{
    private protected RequestContainer()
    {
    }
}

/// <summary>
/// A request container that is a native form, whose submission set a request sends as the browser builds it.
/// </summary>
/// <typeparam name="TModel">The model the form's block was rendered for.</typeparam>
public sealed class RequestForm<TModel> : RequestContainer<TModel>
{
    private RequestForm()
    {
    }
}
