using System;
using System.Text.Json;
using Alis.Reactive.Components;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endif

namespace Alis.Reactive.Hosting;

public sealed class ReactivePlan<TModel>
{
    internal ReactivePlan(PlanBuild build)
    {
        Build = build;
    }

    internal PlanBuild Build { get; }

#if NET48
    public Rendered<TComponent> Render<TComponent>(string id, IHtmlString html)
#else
    public Rendered<TComponent> Render<TComponent>(string id, IHtmlContent html)
#endif
        where TComponent : Component, new()
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A component's element has the id the developer chose.", nameof(id));
        return new Rendered<TComponent>(Build, id, html);
    }

    internal string Json() => PlanJson.Write(Build.Document());
}

#if NET48
public sealed class Rendered<TComponent> : IHtmlString
#else
public sealed class Rendered<TComponent> : IHtmlContent
#endif
    where TComponent : Component, new()
{
    private readonly PlanBuild _build;
    private readonly string _id;
#if NET48
    private readonly IHtmlString _html;

    internal Rendered(PlanBuild build, string id, IHtmlString html)
#else
    private readonly IHtmlContent _html;

    internal Rendered(PlanBuild build, string id, IHtmlContent html)
#endif
    {
        _build = build;
        _id = id;
        _html = html;
    }

    public Rendered<TComponent> Reactive(Func<TComponent, Event> raised, Action<Pipeline> pipeline)
    {
        Trigger.On<TComponent, NoPayload>(_build, _id, raised, [], (_, steps) => pipeline(steps));
        return this;
    }

    public Rendered<TComponent> Reactive<TArgs>(Func<TComponent, Event<TArgs>> raised, Action<TArgs, Pipeline> pipeline)
        where TArgs : JsObject, new()
    {
        Trigger.On(_build, _id, raised, [PathStep.Index(0)], pipeline);
        return this;
    }

#if NET48
    public string ToHtmlString() => _html.ToHtmlString();
#else
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => _html.WriteTo(writer, encoder);
#endif
}

public static class PlanHtml
{
    private const string ScriptOpen = "<script type=\"application/json\" data-alis-plan>";
    private const string ScriptClose = "</script>";

#if NET48
    public static ReactivePlan<TModel> ReactivePlan<TModel>(this HtmlHelper<TModel> html)
    {
        if (html == null) throw new ArgumentNullException(nameof(html));
        return new ReactivePlan<TModel>(new PlanBuild(PlanId<TModel>(), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    public static IHtmlString RenderPlan<TModel>(this HtmlHelper html, ReactivePlan<TModel> plan)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        return new HtmlString(ScriptOpen + plan.Json() + ScriptClose);
    }
#else
    public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var values = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        return new ReactivePlan<TModel>(new PlanBuild(PlanId<TModel>(), values));
    }

    public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper html, ReactivePlan<TModel> plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new HtmlString(ScriptOpen + plan.Json() + ScriptClose);
    }
#endif

    private static string PlanId<TModel>() => typeof(TModel).FullName ?? throw new InvalidOperationException("A plan's model is a named type.");
}
