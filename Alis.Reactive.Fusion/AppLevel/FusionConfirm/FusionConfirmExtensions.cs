using Alis.Reactive.Descriptors.Commands;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.AppLevel
{
    public static class FusionConfirmExtensions
    {
        public static ComponentRef<FusionConfirm, TModel> SetContent<TModel>(
            this ComponentRef<FusionConfirm, TModel> self, string message)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("content"), value: message)
                       .Emit(new CallVoidMutation("dataBind"));
        }

        public static ComponentRef<FusionConfirm, TModel> Show<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.Emit(new CallVoidMutation("show"));

        public static ComponentRef<FusionConfirm, TModel> Hide<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.Emit(new CallVoidMutation("hide"));

        public static string IsVisible<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => $"ref:{self.TargetId}.visible";

        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
            => new HtmlString($"<div id=\"{FusionConfirm.ElementId}\"></div>\n");
    }
}
