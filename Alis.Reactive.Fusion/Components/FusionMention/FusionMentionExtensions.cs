using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionMentionExtensions
    {
        private static readonly ComponentMethod SearchMethod =
            ComponentMethod.Named("search").WithArgs<string, int, int>();

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        public static ComponentRef<FusionMention, TModel> Search<TModel>(
            this ComponentRef<FusionMention, TModel> self,
            string text,
            int positionX,
            int positionY)
            where TModel : class
            => self.EmitCall(
                SearchMethod,
                new List<ValueExpression>
                {
                    ValueExpression.Literal(text),
                    ValueExpression.Literal(positionX),
                    ValueExpression.Literal(positionY)
                });

        public static ComponentRef<FusionMention, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMention, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        public static ComponentRef<FusionMention, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMention, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);
    }
}
