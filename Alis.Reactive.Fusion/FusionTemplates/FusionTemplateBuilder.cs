using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Fusion.FusionTemplates;

/// <summary>
/// A template a Fusion component renders for each of its items: a root element, the elements inside it and the
/// members of the item they show.
/// </summary>
/// <remarks>
/// A member of the item shows as text, and one that holds null or that the item lacks shows nothing. A read past a
/// member the item lacks shows nothing there, and the component's other items render. The view's own text, classes
/// and attributes are written as the view writes them, and <see cref="Raw"/> is the one door for markup.
/// <see cref="Render{TViewModel}"/> writes the text the component's template option takes.
/// </remarks>
/// <typeparam name="TModel">The class of the item, as the app's JSON writes it.</typeparam>
public sealed class FusionTemplateBuilder<TModel>
{
    private readonly TemplateParts<TModel> _parts = new();
    private readonly List<string> _classes = [];
    private readonly List<string> _attributes = [];
    private string _id = "";

    internal FusionTemplateBuilder()
    {
    }

    /// <summary>Sets the id of the template's root element.</summary>
    /// <param name="id">The id the view gives the root element.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Id(string id)
        => Sets(() => _id = " id=\"" + TemplateParts<TModel>.Encoded(TemplateParts<TModel>.Given(id, nameof(id))) + "\"");

    /// <summary>Adds a CSS class to the template's root element.</summary>
    /// <param name="cssClass">The class name.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Class(string cssClass)
        => Sets(() => _classes.Add(TemplateParts<TModel>.Given(cssClass, nameof(cssClass))));

    /// <summary>Sets an attribute of the template's root element to the view's own text.</summary>
    /// <remarks>An attribute whose name starts with <c>on</c> is refused where the view renders, since a view carries no JavaScript.</remarks>
    /// <param name="name">The attribute's name.</param>
    /// <param name="value">The attribute's text.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Attr(string name, string value)
        => Sets(() => _attributes.Add(Attribute(name, value)));

    /// <summary>Adds a member of the item, shown as text.</summary>
    /// <remarks>A member that holds null, or that the item lacks, shows nothing.</remarks>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Text<TProperty>(Expression<Func<TModel, TProperty>> property)
        => Sets(() => _parts.Read(property));

    /// <summary>Adds the view's own text, written as HTML text.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Text(string text)
        => Sets(() => _parts.Text(text));

    /// <summary>Adds a <c>span</c> that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property)
        => Sets(() => _parts.Span(property, ""));

    /// <summary>Adds a <c>span</c> with a CSS class that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <param name="css">The span's CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css)
        => Sets(() => _parts.Span(property, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds a <c>span</c> that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Span(string text)
        => Sets(() => _parts.Span(text, ""));

    /// <summary>Adds a <c>span</c> with a CSS class that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="css">The span's CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Span(string text, string css)
        => Sets(() => _parts.Span(text, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds an <c>img</c> whose <c>src</c> is a member of the item.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="srcProperty">The member of the item that holds the image's URL.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty)
        => Sets(() => _parts.Img(srcProperty, "", ""));

    /// <summary>Adds an <c>img</c> with a CSS class whose <c>src</c> is a member of the item.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="srcProperty">The member of the item that holds the image's URL.</param>
    /// <param name="css">The image's CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css)
        => Sets(() => _parts.Img(srcProperty, TemplateParts<TModel>.Given(css, nameof(css)), ""));

    /// <summary>Adds an <c>img</c> with a CSS class and the view's alternative text, whose <c>src</c> is a member of the item.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="srcProperty">The member of the item that holds the image's URL.</param>
    /// <param name="css">The image's CSS classes, separated by spaces.</param>
    /// <param name="alt">The image's alternative text.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css, string alt)
        => Sets(() => _parts.Img(srcProperty, TemplateParts<TModel>.Given(css, nameof(css)), TemplateParts<TModel>.Given(alt, nameof(alt))));

    /// <summary>Adds a <c>div</c> whose content the view writes with these doors, over the same item.</summary>
    /// <param name="build">The div's content.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> build)
        => Sets(() => _parts.Div(build));

    /// <summary>Adds a badge that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <param name="css">The badge's CSS classes, separated by spaces: EJ2's <c>e-badge</c> unless the view gives others.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge")
        => Sets(() => _parts.Span(property, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds a badge that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="css">The badge's CSS classes, separated by spaces: EJ2's <c>e-badge</c> unless the view gives others.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Badge(string text, string css = "e-badge")
        => Sets(() => _parts.Span(text, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds an icon: a <c>span</c> with EJ2's icon class.</summary>
    /// <param name="iconName">The icon's CSS class.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Icon(string iconName)
        => Sets(() => _parts.Icon(iconName, ""));

    /// <summary>Adds an icon: a <c>span</c> with EJ2's icon class and further CSS classes.</summary>
    /// <param name="iconName">The icon's CSS class.</param>
    /// <param name="css">Further CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Icon(string iconName, string css)
        => Sets(() => _parts.Icon(iconName, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds a button whose click raises an app event, with one member of its payload filled from the item.</summary>
    /// <remarks>
    /// The page's handler of the event runs with the clicked item's value. The member is a direct member of the
    /// payload, named as the app's JSON names it; a record, a list or a member past another member is refused where
    /// the view renders. An item whose member holds null, or that lacks it, raises the event with that member empty.
    /// </remarks>
    /// <typeparam name="TPayload">The class of the event's payload.</typeparam>
    /// <typeparam name="TProperty">The member's type, the same on the payload and on the item.</typeparam>
    /// <param name="text">The button's text.</param>
    /// <param name="raises">The event, as the app declares it.</param>
    /// <param name="member">The payload's member the button fills.</param>
    /// <param name="from">The item's member the value comes from.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> EventButton<TPayload, TProperty>(string text, AppEvent<TPayload> raises, Expression<Func<TPayload, TProperty>> member, Expression<Func<TModel, TProperty>> from)
        => Sets(() => _parts.EventButton(text, raises, member, from, ""));

    /// <summary>Adds a button with a CSS class whose click raises an app event, with one member of its payload filled from the item.</summary>
    /// <remarks>
    /// The page's handler of the event runs with the clicked item's value. The member is a direct member of the
    /// payload, named as the app's JSON names it; a record, a list or a member past another member is refused where
    /// the view renders. An item whose member holds null, or that lacks it, raises the event with that member empty.
    /// </remarks>
    /// <typeparam name="TPayload">The class of the event's payload.</typeparam>
    /// <typeparam name="TProperty">The member's type, the same on the payload and on the item.</typeparam>
    /// <param name="text">The button's text.</param>
    /// <param name="raises">The event, as the app declares it.</param>
    /// <param name="member">The payload's member the button fills.</param>
    /// <param name="from">The item's member the value comes from.</param>
    /// <param name="css">The button's CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> EventButton<TPayload, TProperty>(string text, AppEvent<TPayload> raises, Expression<Func<TPayload, TProperty>> member, Expression<Func<TModel, TProperty>> from, string css)
        => Sets(() => _parts.EventButton(text, raises, member, from, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds a link whose address and text are members of the item.</summary>
    /// <remarks>An address that is relative, or whose scheme is http, https, mailto or tel, is written as the item holds it; any other is written as <c>#</c>.</remarks>
    /// <typeparam name="THref">The type of the member that holds the address.</typeparam>
    /// <typeparam name="TText">The type of the member shown as the link's text.</typeparam>
    /// <param name="hrefProperty">The member of the item that holds the link's address.</param>
    /// <param name="textProperty">The member of the item shown as the link's text.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Link<THref, TText>(Expression<Func<TModel, THref>> hrefProperty, Expression<Func<TModel, TText>> textProperty)
        => Sets(() => _parts.Link(hrefProperty, textProperty, ""));

    /// <summary>Adds a link with a CSS class whose address and text are members of the item.</summary>
    /// <remarks>An address that is relative, or whose scheme is http, https, mailto or tel, is written as the item holds it; any other is written as <c>#</c>.</remarks>
    /// <typeparam name="THref">The type of the member that holds the address.</typeparam>
    /// <typeparam name="TText">The type of the member shown as the link's text.</typeparam>
    /// <param name="hrefProperty">The member of the item that holds the link's address.</param>
    /// <param name="textProperty">The member of the item shown as the link's text.</param>
    /// <param name="css">The link's CSS classes, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Link<THref, TText>(Expression<Func<TModel, THref>> hrefProperty, Expression<Func<TModel, TText>> textProperty, string css)
        => Sets(() => _parts.Link(hrefProperty, textProperty, TemplateParts<TModel>.Given(css, nameof(css))));

    /// <summary>Adds content the component shows for an item where a condition on the item holds.</summary>
    /// <remarks>
    /// The component evaluates the condition on each item. It compares members of the item with <c>==</c>, <c>!=</c>,
    /// <c>&lt;</c>, <c>&lt;=</c>, <c>&gt;</c> and <c>&gt;=</c>, joins terms with <c>&amp;&amp;</c>, <c>||</c> and
    /// <c>!</c>, and multiplies or divides two members. A constant is <c>true</c>, <c>false</c>, a whole number, or
    /// text of ASCII letters, digits, single spaces, <c>_</c>, <c>-</c> and <c>.</c>; any other constant, <c>+</c> and
    /// <c>-</c>, and arithmetic with a constant are refused where the view renders. A value of the view the condition
    /// reads is read when the view renders.
    /// </remarks>
    /// <param name="condition">The condition on the item.</param>
    /// <param name="then">The content shown where the condition holds.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then)
        => Sets(() => _parts.When(condition, then));

    /// <summary>Adds content the component shows for an item where a condition on the item holds, and other content where it does not.</summary>
    /// <remarks>
    /// The component evaluates the condition on each item. It compares members of the item with <c>==</c>, <c>!=</c>,
    /// <c>&lt;</c>, <c>&lt;=</c>, <c>&gt;</c> and <c>&gt;=</c>, joins terms with <c>&amp;&amp;</c>, <c>||</c> and
    /// <c>!</c>, and multiplies or divides two members. A constant is <c>true</c>, <c>false</c>, a whole number, or
    /// text of ASCII letters, digits, single spaces, <c>_</c>, <c>-</c> and <c>.</c>; any other constant, <c>+</c> and
    /// <c>-</c>, and arithmetic with a constant are refused where the view renders. A value of the view the condition
    /// reads is read when the view renders.
    /// </remarks>
    /// <param name="condition">The condition on the item.</param>
    /// <param name="then">The content shown where the condition holds.</param>
    /// <param name="else">The content shown where it does not.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then, Action<FusionConditionalBuilder<TModel>> @else)
        => Sets(() => _parts.When(condition, then, @else));

    /// <summary>Adds markup the view writes, as the view writes it.</summary>
    /// <remarks>It is written with no encoding, so it holds only markup the app writes, never data.</remarks>
    /// <param name="html">The markup.</param>
    /// <returns>The builder.</returns>
    public FusionTemplateBuilder<TModel> Raw(string html)
        => Sets(() => _parts.Raw(html));

    /// <summary>Writes the template as the text a component's template option takes.</summary>
    /// <remarks>Each member is named as the app's JSON options name it, which the view's plan carries.</remarks>
    /// <typeparam name="TViewModel">The model of the view whose plan the template is written under.</typeparam>
    /// <param name="plan">The view's plan.</param>
    /// <returns>The template's text.</returns>
    public string Render<TViewModel>(ReactivePlan<TViewModel> plan)
        => Written(new PlanTemplateContext<TViewModel>(plan ?? throw new ArgumentNullException(nameof(plan))));

    internal string Written(TemplateContext context)
        => "<div" + _id + TemplateParts<TModel>.Class(string.Join(" ", _classes)) + string.Concat(_attributes) + ">" + _parts.Written(context) + "</div>";

    private static string Attribute(string name, string value)
    {
        var named = TemplateParts<TModel>.Given(name, nameof(name));
        var written = TemplateParts<TModel>.Given(value, nameof(value));
        if (named.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Attr names '{named}', which the browser runs as JavaScript, and a view carries no JavaScript; raise an app event with EventButton, or do the work in a plugin.");
        return " " + named + "=\"" + TemplateParts<TModel>.Encoded(written) + "\"";
    }

    private FusionTemplateBuilder<TModel> Sets(Action written)
    {
        written();
        return this;
    }
}
