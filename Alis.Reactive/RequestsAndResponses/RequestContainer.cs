using System;
using System.Linq.Expressions;
using Alis.Reactive.ValuesAndSources;

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

    /// <summary>Reads a member of the container's model as the container sends it: the value of the input inside the container that binds the member.</summary>
    /// <remarks>
    /// The value has the member's type, so a number typed into a text box reads as that number. An input left blank, or
    /// whose text does not convert to the member's type, reads as a new <typeparamref name="TModel"/> would hold the member.
    /// </remarks>
    /// <param name="member">The member to read, as a member path.</param>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <returns>A source of the member's value.</returns>
    public TypedSource<TValue> Read<TValue>(Expression<Func<TModel, TValue>> member) => throw new NotImplementedException();
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
