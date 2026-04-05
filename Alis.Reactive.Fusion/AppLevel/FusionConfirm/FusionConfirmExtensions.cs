using Alis.Reactive.PlanModel;
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
            return self.EmitSet("content", ValueProducer.Literal(message))
                       .EmitCall("dataBind");
        }

        public static ComponentRef<FusionConfirm, TModel> Show<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.EmitCall("show");

        public static ComponentRef<FusionConfirm, TModel> Hide<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.EmitCall("hide");

        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
            => new HtmlString($"<div id=\"{FusionConfirm.ElementId}\"></div>\n");
    }
}
