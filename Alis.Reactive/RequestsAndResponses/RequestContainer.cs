using System;
using System.Linq.Expressions;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// A request container of a view: the element whose inputs a request sends and whose summary shows that request's failures.
/// </summary>
/// <typeparam name="TModel">The model the container's block was rendered for.</typeparam>
public sealed class RequestContainer<TModel>
{
    private readonly PlanContent _plan;

    internal RequestContainer(string elementId, PlanContent plan)
    {
        ElementId = elementId;
        _plan = plan;
    }

    internal string ElementId { get; }

    /// <summary>Reads a member of the container's model as the container sends it: the value of the input inside the container that binds the member.</summary>
    /// <remarks>
    /// The value has the member's type, so a number typed into a text box reads as that number. An input left blank, or
    /// whose text does not convert to the member's type, reads the member's declared start. A member a present holder
    /// lacks reads its start; a null holder, or a list position or dictionary key the sent body lacks, reads nothing.
    /// A holder missing a required member, or an absent member whose start the JSON contract cannot obtain, stops the pipeline.
    /// The read takes the inputs inside the container when its step runs. Two inputs that bind one name, a missing
    /// container element, or no live view rendering the container for this model stops the pipeline.
    /// </remarks>
    /// <param name="member">The member to read, as a member path.</param>
    /// <typeparam name="TValue">The type of the member.</typeparam>
    /// <returns>A source of the member's value.</returns>
    public TypedSource<TValue> Read<TValue>(Expression<Func<TModel, TValue>> member) => ContainerReads.Of(_plan, ElementId, member);
}

