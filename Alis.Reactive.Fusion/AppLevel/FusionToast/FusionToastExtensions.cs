using Alis.Reactive;
using Alis.Reactive.PlanModel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Notifications;

namespace Alis.Reactive.Fusion.AppLevel
{
    public static class FusionToastExtensions
    {
        private static readonly ComponentProperty<string> TitleProperty =
            ComponentProperty<string>.Named("title");

        private static readonly ComponentProperty<string> ContentProperty =
            ComponentProperty<string>.Named("content");

        private static readonly ComponentProperty<int> TimeoutProperty =
            ComponentProperty<int>.Named("timeOut");

        private static readonly ComponentProperty<bool> ShowCloseButtonProperty =
            ComponentProperty<bool>.Named("showCloseButton");

        private static readonly ComponentProperty<bool> ShowProgressBarProperty =
            ComponentProperty<bool>.Named("showProgressBar");

        private static readonly ComponentProperty<string> CssClassProperty =
            ComponentProperty<string>.Named("cssClass");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ShowMethod =
            ComponentMethod.Named("show");

        private static readonly ComponentMethod HideMethod =
            ComponentMethod.Named("hide");

        // ── Fluent setters (all optional) ──

        public static ComponentRef<FusionToast, TModel> SetTitle<TModel>(
            this ComponentRef<FusionToast, TModel> self, string title)
            where TModel : class
            => self.EmitSet(TitleProperty, ValueExpression.Literal(title));

        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.EmitSet(ContentProperty, ValueExpression.Literal(content));

        public static ComponentRef<FusionToast, TModel> SetTimeout<TModel>(
            this ComponentRef<FusionToast, TModel> self, int ms)
            where TModel : class
            => self.EmitSet(TimeoutProperty, ValueExpression.Literal(ms));

        public static ComponentRef<FusionToast, TModel> ShowCloseButton<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(ShowCloseButtonProperty, ValueExpression.Literal(true));

        public static ComponentRef<FusionToast, TModel> ShowProgressBar<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(ShowProgressBarProperty, ValueExpression.Literal(true));

        // ── Type convenience methods ──

        public static ComponentRef<FusionToast, TModel> Success<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-success"));

        public static ComponentRef<FusionToast, TModel> Warning<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-warning"));

        public static ComponentRef<FusionToast, TModel> Danger<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-danger"));

        public static ComponentRef<FusionToast, TModel> Info<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-info"));

        // ── Actions ──

        public static ComponentRef<FusionToast, TModel> Show<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod)
                   .EmitCall(ShowMethod);

        public static ComponentRef<FusionToast, TModel> Hide<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall(HideMethod);

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
