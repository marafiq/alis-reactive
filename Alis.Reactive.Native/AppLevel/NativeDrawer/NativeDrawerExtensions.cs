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
    /// Pipeline and layout extensions for the <see cref="NativeDrawer"/>.
    /// </summary>
    public static class NativeDrawerExtensions
    {
        private static readonly string[] SizeClasses = { "alis-drawer--sm", "alis-drawer--md", "alis-drawer--lg" };

        private static readonly ComponentMethod RemoveAttributeMethod =
            ComponentMethod.Named("removeAttribute").WithArgs<string>();

        private static readonly ComponentMethod ClassAddMethod =
            ComponentMethod.Mapped("classAdd", "classList.add").WithArgs<string>();

        private static readonly ComponentMethod ClassRemoveMethod =
            ComponentMethod.Mapped("classRemove", "classList.remove").WithArgs<string>();

        /// <summary>
        /// Applies the drawer panel width class through the component contract.
        /// </summary>
        /// <param name="size">The panel width class to apply.</param>
        public static ComponentRef<NativeDrawer, TModel> SetSize<TModel>(
            this ComponentRef<NativeDrawer, TModel> self, DrawerSize size)
            where TModel : class
        {
            foreach (var sizeClass in SizeClasses)
                self.EmitCall(ClassRemoveMethod,
                    new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(sizeClass) });

            var selectedSizeClass = size switch
            {
                DrawerSize.Sm => "alis-drawer--sm",
                DrawerSize.Md => "alis-drawer--md",
                DrawerSize.Lg => "alis-drawer--lg",
                _ => "alis-drawer--md"
            };
            self.EmitCall(ClassAddMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(selectedSizeClass) });
            return self;
        }

        /// <summary>
        /// Opens the drawer, making it visible and accessible.
        /// </summary>
        public static ComponentRef<NativeDrawer, TModel> Open<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            self.EmitCall(ClassAddMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("alis-drawer--visible") });
            self.EmitCall(RemoveAttributeMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("aria-hidden") });
            return self;
        }

        /// <summary>
        /// Closes the drawer, hiding the panel.
        /// </summary>
        public static ComponentRef<NativeDrawer, TModel> Close<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            self.EmitCall(ClassRemoveMethod,
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal("alis-drawer--visible") });
            return self;
        }

        /// <summary>
        /// Renders the drawer HTML element in the layout.
        /// </summary>
        /// <remarks>
        /// Call this once in <c>_Layout.cshtml</c>. The drawer is hidden by default
        /// and opened via <see cref="Open{TModel}"/> in a reactive pipeline.
        /// </remarks>
#if NET48
        public static IHtmlString NativeDrawer(this HtmlHelper html)
        {
            return new MvcHtmlString(
#else
        public static IHtmlContent NativeDrawer(this IHtmlHelper html)
        {
            return new HtmlString(
#endif
                "<aside id=\"" + AppLevel.NativeDrawer.ElementId + "\" class=\"alis-drawer\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-drawer__panel\">\n" +
                "    <div class=\"alis-drawer__header\">\n" +
                "      <h2 id=\"alis-drawer-title\" class=\"alis-drawer__title\"></h2>\n" +
                "      <button id=\"alis-drawer-close\" type=\"button\" class=\"alis-drawer__close\" aria-label=\"Close\">\n" +
                "        <svg width=\"20\" height=\"20\" viewBox=\"0 0 20 20\" fill=\"currentColor\"><path d=\"M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z\"/></svg>\n" +
                "      </button>\n" +
                "    </div>\n" +
                "    <div id=\"alis-drawer-content\" class=\"alis-drawer__content\"></div>\n" +
                "  </div>\n" +
                "</aside>\n");
        }
    }
}
