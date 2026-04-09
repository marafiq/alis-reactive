using Alis.Reactive;
using Alis.Reactive.PlanModel;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Notifications;

namespace Alis.Reactive.Fusion.AppLevel
{
    public static class FusionToastExtensions
    {
        // ── Fluent setters (all optional) ──

        public static ComponentRef<FusionToast, TModel> SetTitle<TModel>(
            this ComponentRef<FusionToast, TModel> self, string title)
            where TModel : class
            => self.EmitSet("title", ValueProducer.Literal(title));

        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.EmitSet("content", ValueProducer.Literal(content));

        public static ComponentRef<FusionToast, TModel> SetTimeout<TModel>(
            this ComponentRef<FusionToast, TModel> self, int ms)
            where TModel : class
            => self.EmitSet("timeOut", ValueProducer.Literal(ms));

        public static ComponentRef<FusionToast, TModel> ShowCloseButton<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("showCloseButton", ValueProducer.Literal(true));

        public static ComponentRef<FusionToast, TModel> ShowProgressBar<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("showProgressBar", ValueProducer.Literal(true));

        // ── Type convenience methods ──

        public static ComponentRef<FusionToast, TModel> Success<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("cssClass", ValueProducer.Literal("e-toast-success"));

        public static ComponentRef<FusionToast, TModel> Warning<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("cssClass", ValueProducer.Literal("e-toast-warning"));

        public static ComponentRef<FusionToast, TModel> Danger<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("cssClass", ValueProducer.Literal("e-toast-danger"));

        public static ComponentRef<FusionToast, TModel> Info<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet("cssClass", ValueProducer.Literal("e-toast-info"));

        // ── Actions ──

        public static ComponentRef<FusionToast, TModel> Show<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind")
                   .EmitCall("show");

        public static ComponentRef<FusionToast, TModel> Hide<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall("hide");

        // ── Layout renderer ──

#if NET48
        public static IHtmlString FusionToast(this HtmlHelper html)
#else
        public static IHtmlContent FusionToast(this IHtmlHelper html)
#endif
        {
            return html.EJS().Toast(AppLevel.FusionToast.ElementId)
                .Target("body")
                .Position(new ToastToastPosition { X = "Right", Y = "Bottom" })
                .NewestOnTop(true)
                .ShowCloseButton(true)
                .Render();
        }
    }
}
