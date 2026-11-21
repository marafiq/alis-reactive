using System;
using System.Linq.Expressions;
using Alis.Reactive.Components;
using Alis.Reactive.Inputs;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Hosting;

public static class InputFields
{
#if NET48
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
#else
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
#endif
    {
        return new InputField<TModel, TValue>(plan, html, member, plan.Build.Names.Of(member));
    }
}

public sealed class InputField<TModel, TValue>
{
    private readonly ReactivePlan<TModel> _plan;
    private readonly InputName _name;

#if NET48
    internal InputField(ReactivePlan<TModel> plan, HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> member, InputName name)
#else
    internal InputField(ReactivePlan<TModel> plan, IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> member, InputName name)
#endif
    {
        _plan = plan;
        _name = name;
        Html = html;
        Member = member;
    }

    public string Id => _name.Id;

    public string Name => _name.Name;

#if NET48
    public HtmlHelper<TModel> Html { get; }
#else
    public IHtmlHelper<TModel> Html { get; }
#endif

    public Expression<Func<TModel, TValue>> Member { get; }

    public string ValueJson => MemberValues.Of(Html, Member) is TValue held ? _plan.Json(held) : "null";

    public string Json<TOptions>(TOptions options) => _plan.Json(options);

#if NET48
    public Rendered<TComponent, TModel> Render<TComponent>(IHtmlString html)
#else
    public Rendered<TComponent, TModel> Render<TComponent>(IHtmlContent html)
#endif
        where TComponent : Component, new()
        => _plan.Render<TComponent>(Id, html);
}
