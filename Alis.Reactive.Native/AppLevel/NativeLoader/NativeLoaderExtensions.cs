using Alis.Reactive;
using Alis.Reactive.PlanModel;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// Pipeline and layout extensions for the <see cref="NativeLoader"/>.
    /// </summary>
    public static class NativeLoaderExtensions
    {
        private static readonly ComponentMethod SetAttributeMethod =
            ComponentMethod.Named("setAttribute").WithArgs<string, string>();

        private static readonly ComponentMethod RemoveAttributeMethod =
            ComponentMethod.Named("removeAttribute").WithArgs<string>();

        private static readonly ComponentMethod ClassAddMethod =
            ComponentMethod.Mapped("classAdd", "classList.add").WithArgs<string>();

        private static readonly ComponentMethod ClassRemoveMethod =
            ComponentMethod.Mapped("classRemove", "classList.remove").WithArgs<string>();

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
            => self.EmitCall(SetAttributeMethod,
                   new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("data-target"), ValueExpression.Literal(targetId) });

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
            => self.EmitCall(SetAttributeMethod,
                   new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("data-timeout"), ValueExpression.Literal(ms.ToString()) });

        /// <summary>
        /// Shows the loader overlay, making it visible and accessible.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeLoader, TModel> Show<TModel>(
            this ComponentRef<NativeLoader, TModel> self)
            where TModel : class
        {
            self.EmitCall(ClassAddMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("alis-loader--visible") });
            self.EmitCall(RemoveAttributeMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("aria-hidden") });
            return self;
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
            self.EmitCall(ClassRemoveMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("alis-loader--visible") });
            self.EmitCall(SetAttributeMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("aria-hidden"), ValueExpression.Literal("true") });
            return self;
        }

        /// <summary>
        /// Renders the loader overlay HTML element in the layout.
        /// </summary>
        /// <remarks>
        /// Call this once in <c>_Layout.cshtml</c>. The loader is hidden by default
        /// and shown via <see cref="Show{TModel}"/> in a reactive pipeline.
        /// </remarks>
        /// <returns>The loader HTML element.</returns>
#if NET48
        public static IHtmlString NativeLoader(this HtmlHelper html)
        {
            return new MvcHtmlString(
#else
        public static IHtmlContent NativeLoader(this IHtmlHelper html)
        {
            return new HtmlString(
#endif
                "<div id=\"" + AppLevel.NativeLoader.ElementId + "\" class=\"alis-loader\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-loader__spinner\"></div>\n" +
                "  <p id=\"alis-loader-message\" class=\"alis-loader__message\"></p>\n" +
                "</div>\n");
        }
    }
}
