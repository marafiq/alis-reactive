using Alis.Reactive;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            => self.Emit(new SetPropMutation("title", CommandValue.FromLiteral(title)));

        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.Emit(new SetPropMutation("content", CommandValue.FromLiteral(content)));

        public static ComponentRef<FusionToast, TModel> SetTimeout<TModel>(
            this ComponentRef<FusionToast, TModel> self, int ms)
            where TModel : class
            => self.Emit(new SetPropMutation("timeOut", CommandValue.FromLiteral(ms.ToString(), coerce: "number")));

        public static ComponentRef<FusionToast, TModel> ShowCloseButton<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("showCloseButton", CommandValue.FromLiteral("true", coerce: "boolean")));

        public static ComponentRef<FusionToast, TModel> ShowProgressBar<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("showProgressBar", CommandValue.FromLiteral("true", coerce: "boolean")));

        // ── Type convenience methods ──

        public static ComponentRef<FusionToast, TModel> Success<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("cssClass", CommandValue.FromLiteral("e-toast-success")));

        public static ComponentRef<FusionToast, TModel> Warning<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("cssClass", CommandValue.FromLiteral("e-toast-warning")));

        public static ComponentRef<FusionToast, TModel> Danger<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("cssClass", CommandValue.FromLiteral("e-toast-danger")));

        public static ComponentRef<FusionToast, TModel> Info<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new SetPropMutation("cssClass", CommandValue.FromLiteral("e-toast-info")));

        // ── Actions ──

        public static ComponentRef<FusionToast, TModel> Show<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"))
                   .Emit(new CallMutation("show"));

        public static ComponentRef<FusionToast, TModel> Hide<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hide"));

        // ── Layout renderer ──

        public static IHtmlContent FusionToast(this IHtmlHelper html)
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
