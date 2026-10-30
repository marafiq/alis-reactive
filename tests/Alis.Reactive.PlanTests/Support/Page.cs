using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;
using Alis.Reactive.InputField;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.PlanTests.Support;

/// <summary>
/// A page's plan, authored the way a view authors it and rendered to the JSON the view would embed. The
/// view's own part, Razor, is the only thing left out: inputs join the plan through the identity
/// <c>Html.InputField</c> derives, and elements through <c>plan.Render</c>.
/// </summary>
internal static class Page
{
    internal static ReactivePlan<TModel> Plan<TModel>() where TModel : class =>
        new(services: null, HtmlFieldPrefix.None, ViewOf<TModel>());

    /// <summary>A partial's plan, rendered for the member of the page's model the prefix names.</summary>
    internal static ReactivePlan<TModel> Partial<TModel>(string fieldPrefix) where TModel : class =>
        new(services: null, fieldPrefix, ViewOf<TModel>());

    // The view a page's plan starts in, named for its model.
    private static string ViewOf<TModel>() => $"/{typeof(TModel).Name}.cshtml";

    internal static TriggerBuilder<TModel> Triggers<TModel>(this ReactivePlan<TModel> plan) where TModel : class =>
        new(plan, plan.Context);

    /// <summary>A text input rendered for a member: the element id and binding name <c>Html.InputField</c> gives it.</summary>
    internal static ReactivePlan<TModel> TextInput<TModel, TValue>(this ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        where TModel : class
    {
        var bindingName = HtmlFieldPrefix.Join(plan.Context.FieldPrefix, ExpressionPathHelper.ToMemberBindingPath(member).Value);
        plan.AddRenderedInput(RenderedInput.Of(InputIdentity.For(member, bindingName, plan.Context.FieldPrefix), new TextInput()));
        return plan;
    }

    internal static RenderedComponent<TComponent, TModel> Element<TComponent, TModel>(this ReactivePlan<TModel> plan, string id)
        where TComponent : IComponent, new()
        where TModel : class =>
        plan.Render<TComponent>(id, Microsoft.AspNetCore.Html.HtmlString.Empty);

    /// <summary>The plan as the view embeds it, parsed.</summary>
    internal static JsonNode Rendered<TModel>(this ReactivePlan<TModel> plan, IServiceProvider? services = null) where TModel : class =>
        JsonNode.Parse(services == null ? plan.Render() : plan.Render(services))!;

    internal static JsonArray Handlers(this JsonNode plan) => plan["handlers"]!.AsArray();

    internal static JsonObject Objects(this JsonNode plan) => plan["objects"]!.AsObject();

    internal static string Pretty(this JsonNode plan) => plan.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

    /// <summary>The node and everything under it, in document order.</summary>
    internal static IEnumerable<JsonNode> Nodes(this JsonNode node)
    {
        yield return node;
        var children = node switch
        {
            JsonObject members => members.Select(member => member.Value),
            JsonArray items => items,
            _ => Enumerable.Empty<JsonNode?>(),
        };
        foreach (var child in children.OfType<JsonNode>())
            foreach (var descendant in child.Nodes())
                yield return descendant;
    }

    /// <summary>A shape as the grammar writes it: <c>nullable&lt;number&gt;</c>, <c>array&lt;string&gt;</c>, <c>object{room:string}</c>.</summary>
    internal static string Describe(this JsonNode shape)
    {
        var kind = shape["kind"]!.GetValue<string>();
        return kind switch
        {
            "array" => $"array<{shape["item"]!.Describe()}>",
            "nullable" => $"nullable<{shape["inner"]!.Describe()}>",
            "object" => "object{" + string.Join(", ", shape["fields"]!.AsObject().Select(field => field.Key + ":" + field.Value!.Describe())) + "}",
            _ => kind,
        };
    }
}

/// <summary>The plain DOM a page is made of, declared the way any brought component is: on the native vendor.</summary>
public sealed class TextInput : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string> Value => Property<string>();

    public Event<InputChange> Change => Event<InputChange>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

public sealed class InputChange : JsObject
{
    public ReadOnly<string> Value => ReadOnly<string>();
}

public sealed class Button : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event Click => Event();
}
