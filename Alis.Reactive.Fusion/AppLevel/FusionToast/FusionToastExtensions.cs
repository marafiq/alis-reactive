using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Notifications;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Provides toast-specific mutations and page-level rendering helpers.
    /// </summary>
    public static class FusionToastExtensions
    {
        private static readonly CapabilityProperty TitleProperty = CapabilityProperty.Named("title");
        private static readonly CapabilityProperty ContentProperty = CapabilityProperty.Named("content");
        private static readonly CapabilityProperty TimeOutProperty = CapabilityProperty.Named("timeOut");
        private static readonly CapabilityProperty ShowCloseButtonProperty = CapabilityProperty.Named("showCloseButton");
        private static readonly CapabilityProperty ShowProgressBarProperty = CapabilityProperty.Named("showProgressBar");
        private static readonly CapabilityProperty CssClassProperty = CapabilityProperty.Named("cssClass");
        private static readonly CapabilityMethod DataBindMethod = CapabilityMethod.Named("dataBind");
        private static readonly CapabilityMethod ShowMethod = CapabilityMethod.Named("show");
        private static readonly CapabilityMethod HideMethod = CapabilityMethod.Named("hide");

        /// <summary>Sets the toast title.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="title">The title text to display.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> SetTitle<TModel>(
            this ComponentRef<FusionToast, TModel> self, string title)
            where TModel : class
            => self.Set(TitleProperty, title);

        /// <summary>Sets the toast content.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="content">The content text to display.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> SetContent<TModel>(
            this ComponentRef<FusionToast, TModel> self, string content)
            where TModel : class
            => self.Set(ContentProperty, content);

        /// <summary>Sets the auto-dismiss timeout for the toast.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="ms">The timeout in milliseconds.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> SetTimeout<TModel>(
            this ComponentRef<FusionToast, TModel> self, int ms)
            where TModel : class
            => self.Set(TimeOutProperty, ms);

        /// <summary>Enables the close button for the toast.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> ShowCloseButton<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(ShowCloseButtonProperty, true);

        /// <summary>Enables the timeout progress bar for the toast.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> ShowProgressBar<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(ShowProgressBarProperty, true);

        /// <summary>Applies the success toast styling preset.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Success<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(CssClassProperty, "e-toast-success");

        /// <summary>Applies the warning toast styling preset.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Warning<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(CssClassProperty, "e-toast-warning");

        /// <summary>Applies the danger toast styling preset.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Danger<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(CssClassProperty, "e-toast-danger");

        /// <summary>Applies the informational toast styling preset.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Info<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Set(CssClassProperty, "e-toast-info");

        /// <summary>Flushes the current toast state and shows the toast.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Show<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Call(DataBindMethod)
                   .Call(ShowMethod);

        /// <summary>Hides the toast.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionToast, TModel> Hide<TModel>(
            this ComponentRef<FusionToast, TModel> self)
            where TModel : class
            => self.Call(HideMethod);

        /// <summary>Renders the page-level Syncfusion toast host.</summary>
        /// <param name="html">The HTML helper used to render the host markup.</param>
        /// <returns>The toast host markup.</returns>
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
