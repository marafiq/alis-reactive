using System;
using System.Linq.Expressions;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Fusion.FusionTemplates;

/// <summary>
/// The content of one branch of a template's condition, which the template's <c>When</c> hands its branch.
/// </summary>
/// <remarks>
/// A member of the item shows as text, and one that holds null or that the item lacks shows nothing; <see cref="Raw"/>
/// is the one door for markup.
/// </remarks>
/// <typeparam name="TModel">The class of the item, as the app's JSON writes it.</typeparam>
public sealed class FusionConditionalBuilder<TModel>
{
    internal FusionConditionalBuilder()
    {
    }

    /// <summary>Adds a <c>span</c> that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property) => throw new NotImplementedException();

    /// <summary>Adds a <c>span</c> with a CSS class that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <param name="css">The span's CSS classes, separated by spaces.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Span<TProperty>(Expression<Func<TModel, TProperty>> property, string css) => throw new NotImplementedException();

    /// <summary>Adds a <c>span</c> that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Span(string text) => throw new NotImplementedException();

    /// <summary>Adds a <c>span</c> with a CSS class that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="css">The span's CSS classes, separated by spaces.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Span(string text, string css) => throw new NotImplementedException();

    /// <summary>Adds a badge that shows a member of the item as text.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="property">The member of the item.</param>
    /// <param name="css">The badge's CSS classes, separated by spaces: EJ2's <c>e-badge</c> unless the view gives others.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Badge<TProperty>(Expression<Func<TModel, TProperty>> property, string css = "e-badge") => throw new NotImplementedException();

    /// <summary>Adds a badge that shows the view's own text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="css">The badge's CSS classes, separated by spaces: EJ2's <c>e-badge</c> unless the view gives others.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Badge(string text, string css = "e-badge") => throw new NotImplementedException();

    /// <summary>Adds an icon: a <c>span</c> with EJ2's icon class.</summary>
    /// <param name="iconName">The icon's CSS class.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Icon(string iconName) => throw new NotImplementedException();

    /// <summary>Adds an icon: a <c>span</c> with EJ2's icon class and further CSS classes.</summary>
    /// <param name="iconName">The icon's CSS class.</param>
    /// <param name="css">Further CSS classes, separated by spaces.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Icon(string iconName, string css) => throw new NotImplementedException();

    /// <summary>Adds a <c>div</c> whose content the view writes with the template's doors, over the same item.</summary>
    /// <param name="build">The div's content.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Div(Action<FusionTemplateBuilder<TModel>> build) => throw new NotImplementedException();

    /// <summary>Adds an <c>img</c> whose <c>src</c> is a member of the item.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="srcProperty">The member of the item that holds the image's URL.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty) => throw new NotImplementedException();

    /// <summary>Adds an <c>img</c> with a CSS class whose <c>src</c> is a member of the item.</summary>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    /// <param name="srcProperty">The member of the item that holds the image's URL.</param>
    /// <param name="css">The image's CSS classes, separated by spaces.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Img<TProperty>(Expression<Func<TModel, TProperty>> srcProperty, string css) => throw new NotImplementedException();

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
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> EventButton<TPayload, TProperty>(string text, AppEvent<TPayload> raises, Expression<Func<TPayload, TProperty>> member, Expression<Func<TModel, TProperty>> from)
        => throw new NotImplementedException();

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
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> EventButton<TPayload, TProperty>(string text, AppEvent<TPayload> raises, Expression<Func<TPayload, TProperty>> member, Expression<Func<TModel, TProperty>> from, string css)
        => throw new NotImplementedException();

    /// <summary>Adds markup the view writes, as the view writes it.</summary>
    /// <remarks>It is written with no encoding, so it holds only markup the app writes, never data.</remarks>
    /// <param name="html">The markup.</param>
    /// <returns>The branch's builder.</returns>
    public FusionConditionalBuilder<TModel> Raw(string html) => throw new NotImplementedException();
}
