using Alis.Reactive;
using Alis.Reactive.Descriptors.Mutations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// Pipeline and layout extensions for the <see cref="NativeLoader"/>.
    /// </summary>
    public static class NativeLoaderExtensions
    {
        /// <summary>
        /// Sets which element the loader should cover.
        /// </summary>
        /// <remarks>
        /// The loader moves inside the target element and covers it fully.
        /// If not called, the loader covers the entire viewport.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The loader component reference.</param>
        /// <param name="targetId">The element ID of the container to cover.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeLoader, TModel> SetTarget<TModel>(
            this ComponentRef<NativeLoader, TModel> self, string targetId)
            where TModel : class
            => self.Emit(new CallMutation("setAttribute",
                   args: new MethodArg[] { new LiteralArg("data-target"), new LiteralArg(targetId) }));

        /// <summary>
        /// Sets an auto-hide timeout so the loader disappears after the specified duration.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The loader component reference.</param>
        /// <param name="ms">Timeout in milliseconds before the loader hides itself.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeLoader, TModel> SetTimeout<TModel>(
            this ComponentRef<NativeLoader, TModel> self, int ms)
            where TModel : class
            => self.Emit(new CallMutation("setAttribute",
                   args: new MethodArg[] { new LiteralArg("data-timeout"), new LiteralArg(ms.ToString()) }));

        /// <summary>
        /// Shows the loader overlay, making it visible and accessible.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeLoader, TModel> Show<TModel>(
            this ComponentRef<NativeLoader, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("add", "classList",
                           new[] { new LiteralArg("alis-loader--visible") }))
                       .Emit(new CallMutation("removeAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden") }));
        }

        /// <summary>
        /// Hides the loader overlay.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeLoader, TModel> Hide<TModel>(
            this ComponentRef<NativeLoader, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("remove", "classList",
                           new[] { new LiteralArg("alis-loader--visible") }))
                       .Emit(new CallMutation("setAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden"), new LiteralArg("true") }));
        }

        /// <summary>
        /// Renders the loader overlay HTML element in the layout.
        /// </summary>
        /// <remarks>
        /// Call this once in <c>_Layout.cshtml</c>. The loader is hidden by default
        /// and shown via <see cref="Show{TModel}"/> in a reactive pipeline.
        /// </remarks>
        /// <returns>The loader HTML element.</returns>
        public static IHtmlContent NativeLoader(this IHtmlHelper html)
        {
            return new HtmlString(
                "<div id=\"" + AppLevel.NativeLoader.ElementId + "\" class=\"alis-loader\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-loader__spinner\"></div>\n" +
                "  <p id=\"alis-loader-message\" class=\"alis-loader__message\"></p>\n" +
                "</div>\n");
        }
    }
}
