#if NET48
using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Web;
using System.Web.Mvc;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Vendors;

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
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member)
        => new(html, plan, member, new InputFieldOptions());

    /// <summary>Binds an input to a member of the view's model and says what the field renders around it.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="member">The member the input binds, as a member path.</param>
    /// <param name="options">The field's label and required marker, or its hidden form.</param>
    /// <returns>The input field.</returns>
    public static InputField<TModel, TValue> InputField<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member, Action<InputFieldOptions> options)
    {
        var chosen = new InputFieldOptions();
        options(chosen);
        return new(html, plan, member, chosen);
    }
}

public sealed partial class InputField<TModel, TValue>
{
    internal InputField(HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Expression<Func<TModel, TValue>> member, InputFieldOptions options)
    {
        Html = html;
        Member = member;
        _plan = plan;
        _options = options;
        var prefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
        var text = MvcText.Of(member);
        var name = html.ViewData.TemplateInfo.GetFullHtmlFieldName(text);
        FieldPrefixes.RefuseAMoveAfterThePlanStarted(plan.StartPrefix, prefix, () => ViewPaths.Of(html), name);
        var live = plan.Writer.Content.Json.LiveForm;
        var lambda = MemberLambda.Of(member, text, name, typeof(TModel));
        _bound = BoundInput.Of(lambda, new MvcNames(prefix, name, HtmlHelper.GenerateIdFromName(name)), plan.Writer.Content.Json, () => JsonSerializer.SerializeToElement(default(TValue), live.GetTypeInfo(typeof(TValue))));
        _classNames = DependencyResolver.Current.GetService<ReactiveClassNames>() ?? new ReactiveClassNames();
        ValueJson = ModelMetadata.FromLambdaExpression(member, html.ViewData).Model is TValue held ? plan.Json(held) : "null";
    }

    /// <summary>Gets the view's HTML helper.</summary>
    public HtmlHelper<TModel> Html { get; }

    /// <summary>Renders the input's element under the field's id and declares its component on it.</summary>
    /// <param name="html">The element's markup.</param>
    /// <typeparam name="TComponent">The component's declaration, an input.</typeparam>
    /// <returns>The rendered component, which writes its markup where the view puts it.</returns>
    public Rendered<TComponent, TModel> Render<TComponent>(IHtmlString html)
        where TComponent : Component, IInputComponent, new()
        => Registers(() => new Rendered<TComponent, TModel>(Id, _plan.Writer, _options.IsHidden ? html : new HtmlString(Wrapper + html.ToHtmlString() + WrapperEnd)));
}
#endif
