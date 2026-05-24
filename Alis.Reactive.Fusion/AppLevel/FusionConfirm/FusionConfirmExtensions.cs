using Alis.Reactive.PlanModel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.AppLevel
{
    public static class FusionConfirmExtensions
    {
        private static readonly ComponentProperty<string> ContentProperty =
            ComponentProperty<string>.Named("content");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod ShowMethod =
            ComponentMethod.Named("show");

        private static readonly ComponentMethod HideMethod =
            ComponentMethod.Named("hide");

        public static ComponentRef<FusionConfirm, TModel> SetContent<TModel>(
            this ComponentRef<FusionConfirm, TModel> self, string message)
            where TModel : class
        {
            return self.EmitSet(ContentProperty, ValueProducer.Literal(message))
                       .EmitCall(DataBindMethod);
        }

        public static ComponentRef<FusionConfirm, TModel> Show<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.EmitCall(ShowMethod);

        public static ComponentRef<FusionConfirm, TModel> Hide<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.EmitCall(HideMethod);

        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
            => new HtmlString($"<div id=\"{FusionConfirm.ElementId}\"></div>\n");
    }
}
