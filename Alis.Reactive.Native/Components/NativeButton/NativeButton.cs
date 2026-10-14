#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;button&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.NativeButton("save-plan", "Save")</c> and wire it with
    /// <c>.Reactive(plan, evt =&gt; evt.Click, (args, p) =&gt; ...)</c>; reach it with
    /// <c>p.Component&lt;NativeButton&gt;("save-plan")</c> and use its members directly:
    /// <code>
    /// var save = p.Component&lt;NativeButton&gt;("save-plan");
    /// save.TextContent.Set("Saved");
    /// save.Focus();
    /// </code>
    /// </remarks>
    public sealed class NativeButton : NativeComponent
    {
        /// <summary>The button text.</summary>
        public Prop<string> TextContent => Prop<string>();

        /// <summary>Moves focus to the button.</summary>
        public void Focus() => Call();

        /// <summary>Raised when the user clicks the button.</summary>
        public TypedEvent<NativeButtonClickArgs> Click =>
            Event<NativeButtonClickArgs>();
    }

    /// <summary>Payload of <see cref="NativeButton.Click"/>; carries no data.</summary>
    public class NativeButtonClickArgs : JsObject
    {
    }

    /// <summary>
    /// Factory extension for rendering explicit-ID native buttons directly in a Razor view.
    /// </summary>
    public static class NativeButtonHtmlExtensions
    {
        /// <summary>
        /// Creates a native button builder with the element ID used by Reactive Plan event wiring.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="elementId">Element ID rendered on the button and used for component lookup.</param>
        /// <param name="text">Button text content.</param>
        /// <returns>Button builder for inline rendering.</returns>
        public static NativeButtonBuilder<TModel> NativeButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html, string elementId, string text)
#else
            this IHtmlHelper<TModel> html, string elementId, string text)
#endif
            where TModel : class
        {
            return new NativeButtonBuilder<TModel>(elementId, text);
        }
    }
}
