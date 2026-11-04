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

internal static class Page
{
    internal static (IHtmlHelper<TModel> Html, ReactivePlan<TModel> Plan) Of<TModel>(Action<IServiceCollection>? registered = null)
        where TModel : class =>
        ViewOf<TModel>(registered ?? Registered.Nothing(), fieldPrefix: "");

    internal static (IHtmlHelper<TModel> Html, ReactivePlan<TModel> Plan) Partial<TModel>(string fieldPrefix)
        where TModel : class =>
        ViewOf<TModel>(Registered.Nothing(), fieldPrefix);

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

    internal static RenderedComponent<TextInput, TModel> TextInput<TModel, TValue>(
        this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        where TModel : class
    {
        var field = html.InputField(plan, member);
        return field.Render<TextInput>(new HtmlString($"<input type=\"text\" id=\"{field.ElementId}\" name=\"{field.BindingPath}\">"));
    }

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

file sealed class NamedView(string path) : IView
{
    public string Path => path;

    public Task RenderAsync(ViewContext context) =>
        throw new NotSupportedException($"{path} is authored by the plan tests, never run.");
}

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
