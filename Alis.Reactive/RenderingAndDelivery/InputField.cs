using System;
using System.Linq.Expressions;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// An input bound to a member of the view's model, for a component's render helper to render: its id, the name it
/// binds under and the value it starts with.
/// </summary>
/// <typeparam name="TModel">The model of the view that renders the input.</typeparam>
/// <typeparam name="TValue">The type of the member the input binds.</typeparam>
public sealed partial class InputField<TModel, TValue>
{
    private readonly ReactivePlan<TModel> _plan;
    private readonly InputFieldOptions _options;
    private readonly BoundInput _bound;
    private readonly ReactiveClassNames _classNames;

    /// <summary>Gets the id the framework gives the input's element: the class that declares the member, then the member's path.</summary>
    public string Id => _bound.Id;

    /// <summary>Gets the name the input binds under: the member's full field name in its view.</summary>
    public string Name => _bound.Name;

    /// <summary>Gets the member the input binds.</summary>
    public Expression<Func<TModel, TValue>> Member { get; }

    /// <summary>Gets the member's value as the plan writes a literal: the state the component starts in.</summary>
    /// <remarks>It is <c>null</c> when an object on the way to the member holds nothing.</remarks>
    public string ValueJson { get; }

    /// <summary>Gets whether the member binds a list: System.Text.Json's built-in handling writes its type as a JSON array, as it writes a list or an array.</summary>
    /// <remarks>A converter the app's JSON options register does not change it; a type that names its own converter is no list.</remarks>
    public bool BindsAList => _bound.BindsAList;

    /// <summary>Writes the options a render helper starts its component with, as the plan writes a literal.</summary>
    /// <param name="options">The options to write.</param>
    /// <returns>The options' JSON.</returns>
    public string Json<TOptions>(TOptions options) => _plan.Json(options);

    /// <summary>Names a member of the component's items as their JSON names it, for a component that finds a field of its data by name.</summary>
    /// <remarks>The name is the one the items handed with <see cref="Json{TOptions}"/> and the app's endpoints hold: its <c>JsonPropertyName</c>, else the name the app's JSON options give it.</remarks>
    /// <param name="member">One member of the item, as <c>(Allergy a) =&gt; a.Name</c>.</param>
    /// <returns>The member's name in the items' data.</returns>
    public string JsonName<TItem, TMember>(Expression<Func<TItem, TMember>> member)
        => MemberPath.NameOfItemsMember(member ?? throw new ArgumentNullException(nameof(member)), _plan.Writer.Content.Json.LiveForm);

    private Rendered<TComponent, TModel> Registers<TComponent>(Func<Rendered<TComponent, TModel>> render)
        where TComponent : Component, IInputComponent, new()
    {
        if (_plan.Writer.Content.HasRendered(Id))
        {
            throw new InvalidOperationException(
                $"Input '{Name}' renders element id '{Id}' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor.");
        }

        var rendered = render();
        if (_bound.JoinsTheBody) ModelPlugin.Binds(_plan.Writer.Content, Id, _bound.Binding(rendered.Component.Input, _classNames.InvalidInput, _options.IsHidden));
        return rendered;
    }

    private string Wrapper => FieldMarkup.Open(_classNames, _options, Id);

    private string WrapperEnd => FieldMarkup.Close(_classNames, _bound.MessageId);
}
