using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.PlanTests.Support;

/// <summary>
/// A view and its plan, authored with the helpers a view authors them with (<c>Html.ReactivePlan</c>,
/// <c>Html.On</c>, <c>Html.InputField</c>) and rendered to the JSON <c>Html.RenderPlan</c> embeds. The helper is
/// the one MVC gives a view, in a request whose services the app registered. Razor is the only thing left out:
/// the view is named, not run.
/// </summary>
internal static class Page
{
    /// <summary>A page's view and plan, with the services its app registered beyond MVC: the validators.</summary>
    internal static (IHtmlHelper<TModel> Html, ReactivePlan<TModel> Plan) Of<TModel>(Action<IServiceCollection>? registered = null)
        where TModel : class =>
        ViewOf<TModel>(registered ?? Registered.Nothing(), fieldPrefix: "");

    /// <summary>A partial's view and plan, rendered for the member of the page's model the prefix names.</summary>
    internal static (IHtmlHelper<TModel> Html, ReactivePlan<TModel> Plan) Partial<TModel>(string fieldPrefix)
        where TModel : class =>
        ViewOf<TModel>(Registered.Nothing(), fieldPrefix);

    // The app's host, as every app starts it, with MVC and what the app registered; the request is a scope of it.
    private static (IHtmlHelper<TModel>, ReactivePlan<TModel>) ViewOf<TModel>(Action<IServiceCollection> registered, string fieldPrefix)
        where TModel : class
    {
        var app = WebApplication.CreateBuilder();
        app.Services.AddControllersWithViews();
        registered(app.Services);
        var services = app.Build().Services.CreateScope().ServiceProvider;
        var http = new DefaultHttpContext { RequestServices = services };
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var viewData = new ViewDataDictionary<TModel>(services.GetRequiredService<IModelMetadataProvider>(), action.ModelState);
        viewData.TemplateInfo.HtmlFieldPrefix = fieldPrefix;
        var tempData = services.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(http);
        var view = new ViewContext(action, new NamedView($"/{typeof(TModel).Name}.cshtml"), viewData, tempData, TextWriter.Null, new HtmlHelperOptions());
        var html = services.GetRequiredService<IHtmlHelper<TModel>>();
        ((IViewContextAware)html).Contextualize(view);
        return (html, html.ReactivePlan());
    }

    /// <summary>
    /// A text input rendered for a member, as a brought component's render helper renders it: into the field
    /// <c>Html.InputField</c> starts, with the element id and binding name the field gives it.
    /// </summary>
    internal static RenderedComponent<TextInput, TModel> TextInput<TModel, TValue>(
        this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        where TModel : class
    {
        var field = html.InputField(plan, member);
        return field.Render<TextInput>(new HtmlString($"<input type=\"text\" id=\"{field.ElementId}\" name=\"{field.BindingPath}\">"));
    }

    /// <summary>The plan as the view embeds it, parsed.</summary>
    internal static JsonNode Rendered<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan) where TModel : class
    {
        using var writer = new StringWriter();
        html.RenderPlan(plan).WriteTo(writer, HtmlEncoder.Default);
        var markup = writer.ToString();
        const string script = "<script type=\"application/json\" data-reactive-plan>";
        var start = markup.IndexOf(script, StringComparison.Ordinal) + script.Length;
        var end = markup.IndexOf("</script>", start, StringComparison.Ordinal);
        return JsonNode.Parse(markup[start..end])!;
    }

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

/// <summary>The view a plan starts in, by the path MVC names it with. The plan tests author its plan and never run it.</summary>
file sealed class NamedView(string path) : IView
{
    public string Path => path;

    public Task RenderAsync(ViewContext context) =>
        throw new NotSupportedException($"{path} is authored by the plan tests, never run.");
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
