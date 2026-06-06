using Alis.Reactive;
using Alis.Reactive.PlanModel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Notifications;

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Pipeline and layout extensions for the <see cref="FusionToast"/>.
    /// </summary>
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

        /// <summary>
        /// Writes the Syncfusion Toast title through the component contract.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> SetTitle<TModel>(
            this ComponentRef<FusionToast, TModel> self, string title)
            where TModel : class
            => self.EmitSet(TitleProperty, ValueExpression.Literal(title));

        /// <summary>
        /// Writes the Syncfusion Toast content through the component contract.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.EmitSet(ContentProperty, ValueExpression.Literal(content));

        /// <summary>
        /// Sets the Syncfusion Toast display duration in milliseconds.
        /// </summary>
        /// <param name="ms">Display duration in milliseconds.</param>
        public static ComponentRef<FusionToast, TModel> SetTimeout<TModel>(
            this ComponentRef<FusionToast, TModel> self, int ms)
            where TModel : class
            => self.EmitSet(TimeoutProperty, ValueExpression.Literal(ms));

        /// <summary>
        /// Enables the Syncfusion Toast close button.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> ShowCloseButton<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(ShowCloseButtonProperty, ValueExpression.Literal(true));

        /// <summary>
        /// Enables the Syncfusion Toast progress bar.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> ShowProgressBar<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(ShowProgressBarProperty, ValueExpression.Literal(true));

        /// <summary>
        /// Applies Syncfusion success severity styling.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Success<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-success"));

        /// <summary>
        /// Applies Syncfusion warning severity styling.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Warning<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-warning"));

        /// <summary>
        /// Applies Syncfusion danger severity styling.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Danger<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-danger"));

        /// <summary>
        /// Applies Syncfusion informational severity styling.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Info<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitSet(CssClassProperty, ValueExpression.Literal("e-toast-info"));

        /// <summary>
        /// Applies pending Toast property updates and shows the toast.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Show<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod)
                   .EmitCall(ShowMethod);

        /// <summary>
        /// Hides the Syncfusion Toast.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> Hide<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.EmitCall(HideMethod);

        /// <summary>
        /// Renders the page-level Syncfusion Toast host in the layout.
        /// </summary>
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
