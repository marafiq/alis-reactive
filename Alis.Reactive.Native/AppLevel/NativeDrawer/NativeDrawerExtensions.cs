using Alis.Reactive;
using Alis.Reactive.Descriptors.Mutations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.AppLevel
{
    public static class NativeDrawerExtensions
    {
        private static readonly string[] SizeClasses = { "alis-drawer--sm", "alis-drawer--md", "alis-drawer--lg" };

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

        public static ComponentRef<NativeDrawer, TModel> Open<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("add", "classList",
                           new[] { new LiteralArg("alis-drawer--visible") }))
                       .Emit(new CallMutation("removeAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden") }));
        }

        public static ComponentRef<NativeDrawer, TModel> Close<TModel>(
            this ComponentRef<NativeDrawer, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("remove", "classList",
                       new[] { new LiteralArg("alis-drawer--visible") }));
        }

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
