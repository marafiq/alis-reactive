using Alis.Reactive;
using Alis.Reactive.Descriptors.Mutations;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.AppLevel
{
    public static class NativeLoaderExtensions
    {
        /// <summary>
        /// Sets the target element ID the loader should cover.
        /// Loader moves inside the target and covers it fully.
        /// If not called, covers the viewport.
        /// </summary>
        public static ComponentRef<NativeLoader, TModel> SetTarget<TModel>(
            this ComponentRef<NativeLoader, TModel> self, string targetId)
            where TModel : class
            => self.Emit(new CallMutation("setAttribute",
                   args: new MethodArg[] { new LiteralArg("data-target"), new LiteralArg(targetId) }));

        /// <summary>
        /// Sets auto-hide timeout in milliseconds.
        /// Loader hides automatically after the specified duration.
        /// </summary>
        public static ComponentRef<NativeLoader, TModel> SetTimeout<TModel>(
            this ComponentRef<NativeLoader, TModel> self, int ms)
            where TModel : class
            => self.Emit(new CallMutation("setAttribute",
                   args: new MethodArg[] { new LiteralArg("data-timeout"), new LiteralArg(ms.ToString()) }));

        /// <summary>Shows the loader overlay.</summary>
        public static ComponentRef<NativeLoader, TModel> Show<TModel>(
            this ComponentRef<NativeLoader, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("add", "classList",
                           new[] { new LiteralArg("alis-loader--visible") }))
                       .Emit(new CallMutation("removeAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden") }));
        }

        /// <summary>Hides the loader overlay.</summary>
        public static ComponentRef<NativeLoader, TModel> Hide<TModel>(
            this ComponentRef<NativeLoader, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("remove", "classList",
                           new[] { new LiteralArg("alis-loader--visible") }))
                       .Emit(new CallMutation("setAttribute",
                           args: new MethodArg[] { new LiteralArg("aria-hidden"), new LiteralArg("true") }));
        }

        /// <summary>Renders the loader overlay element in the layout.</summary>
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
