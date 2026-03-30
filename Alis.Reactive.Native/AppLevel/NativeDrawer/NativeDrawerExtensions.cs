using Alis.Reactive;
using Alis.Reactive.Descriptors.Mutations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// Pipeline and layout extensions for the <see cref="NativeDrawer"/>.
    /// </summary>
    public static class NativeDrawerExtensions
    {
        private static readonly string[] SizeClasses = { "alis-drawer--sm", "alis-drawer--md", "alis-drawer--lg" };

        /// <summary>
        /// Sets the drawer panel width.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The drawer component reference.</param>
        /// <param name="size">The desired panel width.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeDrawer, TModel> SetSize<TModel>(
            this ComponentRef<NativeDrawer, TModel> self, DrawerSize size)
            where TModel : class
        {
            // Remove all size classes, then add the requested one
            foreach (var cls in SizeClasses)
                self = self.Emit(new CallMutation("remove", "classList",
                    new[] { new LiteralArg(cls) }));

            var sizeClass = size switch
            {
                DrawerSize.Sm => "alis-drawer--sm",
                DrawerSize.Md => "alis-drawer--md",
                DrawerSize.Lg => "alis-drawer--lg",
                _ => "alis-drawer--md"
            };
            return self.Emit(new CallMutation("add", "classList",
                new[] { new LiteralArg(sizeClass) }));
        }

        /// <summary>
        /// Opens the drawer, making it visible and accessible.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeDrawer, TModel> Open<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("add", "classList",
                           new[] { new LiteralArg("alis-drawer--visible") }))
                       .Emit(new CallMutation("removeAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden") }));
        }

        /// <summary>
        /// Closes the drawer, hiding the panel.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeDrawer, TModel> Close<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("remove", "classList",
                       new[] { new LiteralArg("alis-drawer--visible") }));
        }

        /// <summary>
        /// Renders the drawer HTML element in the layout.
        /// </summary>
        /// <remarks>
        /// Call this once in <c>_Layout.cshtml</c>. The drawer is hidden by default
        /// and opened via <see cref="Open{TModel}"/> in a reactive pipeline.
        /// </remarks>
        /// <returns>The drawer HTML element.</returns>
        public static IHtmlContent NativeDrawer(this IHtmlHelper html)
        {
            return new HtmlString(
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
