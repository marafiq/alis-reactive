#if NET10_0_OR_GREATER
using System;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The door of a view to an input bound to a member of its model.
/// </summary>
public static class InputFields
{
    /// <summary>Binds an input to a member of the view's model, for a component's render helper to render.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <returns>The input field.</returns>
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        => new(html, plan, member, new InputFieldOptions());

    /// <summary>Binds an input to a member of the view's model and says what the field renders around it.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <param name="options">The field's label and required marker, or its hidden form.</param>
    /// <returns>The input field.</returns>
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member, Action<InputFieldOptions> options)
    {
        var chosen = new InputFieldOptions();
        options(chosen);
        return new(html, plan, member, chosen);
    }
}

public sealed partial class InputField<TModel, TValue>
{
    internal InputField(IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member, InputFieldOptions options)
    {
        Html = html;
        Member = member;
        _plan = plan;
        _options = options;
        var prefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
        var text = MvcText.Of(member);
        var name = html.ViewData.TemplateInfo.GetFullHtmlFieldName(text);
        FieldPrefixes.RefuseAMoveAfterThePlanStarted(plan.StartPrefix, prefix, () => ViewPaths.Of(html), name);
        var app = plan.Writer.Content.Json.App;
        var lambda = MemberLambda.Of(member, text, name, typeof(TModel));
        _bound = BoundInput.Of(lambda, new MvcNames(prefix, name, html.GenerateIdFromName(name)), app, () => JsonSerializer.SerializeToElement(default(TValue), app.GetTypeInfo(typeof(TValue))));
        var services = html.ViewContext.HttpContext.RequestServices;
        _classNames = services.GetService<ReactiveClassNames>() ?? new ReactiveClassNames();
        ValueJson = services.GetRequiredService<ModelExpressionProvider>().CreateModelExpression(html.ViewData, member).Model is TValue held ? plan.Json(held) : "null";
    }

    /// <summary>Gets the view's HTML helper.</summary>
    public IHtmlHelper<TModel> Html { get; }

    /// <summary>Renders the input's element under the field's id and declares its component on it.</summary>
    /// <param name="html">The element's markup.</param>
    /// <typeparam name="TComponent">The component's declaration, an input.</typeparam>
    /// <returns>The rendered component, which writes its markup where the view puts it.</returns>
    public Rendered<TComponent, TModel> Render<TComponent>(IHtmlContent html)
        where TComponent : Component, IInputComponent, new()
        => Registers(() => new Rendered<TComponent, TModel>(Id, _plan.Writer, _options.IsHidden ? html : new HtmlContentBuilder().AppendHtml(Wrapper).AppendHtml(html).AppendHtml(WrapperEnd)));
}
#endif
