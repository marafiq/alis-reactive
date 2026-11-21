using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Fusion.FusionTemplates;

internal sealed class TemplateParts<TModel>
{
    private readonly List<Func<TemplateContext, string>> _parts = [];

    internal string Written(TemplateContext context) => string.Concat(_parts.Select(part => part(context)));

    internal void Read<TProperty>(Expression<Func<TModel, TProperty>> property)
        => Reads(property, read => read);

    internal void Text(string text) => _parts.Add(_ => Encoded(Given(text, nameof(text))));

    internal void Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css)
        => Reads(property, read => "<span" + Class(css) + ">" + read + "</span>");

    internal void Span(string text, string css)
    {
        var shown = Encoded(Given(text, nameof(text)));
        _parts.Add(_ => "<span" + Class(css) + ">" + shown + "</span>");
    }

    internal void Icon(string iconName, string css)
    {
        var classes = Joined("e-icons", Given(iconName, nameof(iconName)), css);
        _parts.Add(_ => "<span" + Class(classes) + "></span>");
    }

    internal void Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css, string alt)
        => Reads(srcProperty, read => "<img src=\"" + read + "\"" + Class(css) + (alt.Length == 0 ? "" : " alt=\"" + Encoded(alt) + "\"") + ">");

    internal void Link<THref, TText>(Expression<Func<TModel, THref>> hrefProperty, Expression<Func<TModel, TText>> textProperty, string css)
    {
        _ = hrefProperty ?? throw new ArgumentNullException(nameof(hrefProperty));
        _ = textProperty ?? throw new ArgumentNullException(nameof(textProperty));
        _parts.Add(context => "<a href=\"" + TemplateReads.Url(context.Path(hrefProperty)) + "\"" + Class(css) + ">" + TemplateReads.Text(context.Path(textProperty)) + "</a>");
    }

    internal void EventButton<TPayload, TProperty>(string text, AppEvent<TPayload> raises, Expression<Func<TPayload, TProperty>> member, Expression<Func<TModel, TProperty>> from, string css)
    {
        var shown = Encoded(Given(text, nameof(text)));
        _ = raises ?? throw new ArgumentNullException(nameof(raises));
        _ = member ?? throw new ArgumentNullException(nameof(member));
        _ = from ?? throw new ArgumentNullException(nameof(from));
        var classes = Joined("e-btn", css);
        _parts.Add(context => "<button type=\"button\"" + Class(classes) + " " + context.Raises(raises, member, TemplateReads.Text(context.Path(from))) + ">" + shown + "</button>");
    }

    internal void Div(Action<FusionTemplateBuilder<TModel>> build)
    {
        var div = new FusionTemplateBuilder<TModel>();
        (build ?? throw new ArgumentNullException(nameof(build)))(div);
        _parts.Add(div.Written);
    }

    internal void When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then)
        => Branches(condition, then, _ => "");

    internal void When(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then, Action<FusionConditionalBuilder<TModel>> otherwise)
    {
        var shown = Branch(otherwise ?? throw new ArgumentNullException(nameof(otherwise)));
        Branches(condition, then, context => "${else}" + shown.Written(context));
    }

    internal void Raw(string html)
    {
        var written = Given(html, nameof(html));
        _parts.Add(_ => written);
    }

    internal static string Class(string css) => css.Length == 0 ? "" : " class=\"" + Encoded(css) + "\"";

    internal static string Encoded(string text) => WebUtility.HtmlEncode(text);

    internal static string Given(string text, string name) => text ?? throw new ArgumentNullException(name);

    private static string Joined(params string[] classes) => string.Join(" ", classes.Where(css => css.Length > 0));

    private static FusionConditionalBuilder<TModel> Branch(Action<FusionConditionalBuilder<TModel>> build)
    {
        var branch = new FusionConditionalBuilder<TModel>();
        build(branch);
        return branch;
    }

    private void Branches(Expression<Func<TModel, bool>> condition, Action<FusionConditionalBuilder<TModel>> then, Func<TemplateContext, string> otherwise)
    {
        _ = condition ?? throw new ArgumentNullException(nameof(condition));
        var shown = Branch(then ?? throw new ArgumentNullException(nameof(then)));
        _parts.Add(context => "${if(" + TemplateConditions.Written(condition, context) + ")}" + shown.Written(context) + otherwise(context) + "${/if}");
    }

    private void Reads<TProperty>(Expression<Func<TModel, TProperty>> property, Func<string, string> element)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        _parts.Add(context => element(TemplateReads.Text(context.Path(property))));
    }
}
