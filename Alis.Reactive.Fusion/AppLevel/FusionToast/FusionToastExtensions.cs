using System;
using System.Linq.Expressions;
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
    /// <summary>
    /// Configures, shows, hides, and renders the page-level <see cref="FusionToast"/>.
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
        /// Sets the toast title from the response, such as the name of what the server just changed.
        /// </summary>
        /// <param name="self">The page's toast.</param>
        /// <param name="source">The response body of <c>OnSuccess</c> or <c>OnError</c>.</param>
        /// <param name="title">The response member that holds the title, such as <c>r =&gt; r.Title</c>.</param>
        public static ComponentRef<FusionToast, TModel> SetTitle<TModel, TResponse>(
            this ComponentRef<FusionToast, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, string>> title)
            where TModel : class
            where TResponse : class
            => self.EmitSet(TitleProperty, source.Read(title).ToValueExpression());

        /// <summary>
        /// Writes the Syncfusion Toast content through the component contract.
        /// </summary>
        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.EmitSet(ContentProperty, ValueExpression.Literal(content));

        /// <summary>
        /// Sets the toast content from the response, so the toast says what the server reports.
        /// </summary>
        /// <param name="self">The page's toast.</param>
        /// <param name="source">The response body of <c>OnSuccess</c> or <c>OnError</c>.</param>
        /// <param name="content">The response member that holds the message, such as <c>r =&gt; r.Message</c>.</param>
        /// <remarks>
        /// <code>s.Component&lt;FusionToast&gt;().SetContent(json, r =&gt; r.Message).Success().Show();</code>
        /// </remarks>
        public static ComponentRef<FusionToast, TModel> SetContent<TModel, TResponse>(
            this ComponentRef<FusionToast, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, string>> content)
            where TModel : class
            where TResponse : class
            => self.EmitSet(ContentProperty, source.Read(content).ToValueExpression());

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
