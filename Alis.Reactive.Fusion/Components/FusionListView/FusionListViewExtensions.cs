using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive pipeline extensions for <see cref="FusionListView"/>.
    /// </summary>
    public static class FusionListViewExtensions
    {
        private static readonly ComponentMethod SelectTextMethod =
            ComponentMethod.Mapped("selectText", "selectItem").WithArgs<string>();

        private static readonly ComponentMethod UnselectTextMethod =
            ComponentMethod.Mapped("unselectText", "unselectItem").WithArgs<string>();

        private static readonly ComponentMethod CheckAllItemsMethod =
            ComponentMethod.Named("checkAllItems");

        private static readonly ComponentMethod UncheckAllItemsMethod =
            ComponentMethod.Named("uncheckAllItems");

        /// <summary>Selects an item by visible text for a primitive string ListView data source.</summary>
        public static ComponentRef<FusionListView, TModel> SelectText<TModel>(
            this ComponentRef<FusionListView, TModel> self,
            string text)
            where TModel : class
            => self.EmitCall(
                SelectTextMethod,
                new List<ValueExpression> { ValueExpression.Literal(text) });

        /// <summary>Clears selection for an item by visible text for a primitive string ListView data source.</summary>
        public static ComponentRef<FusionListView, TModel> UnselectText<TModel>(
            this ComponentRef<FusionListView, TModel> self,
            string text)
            where TModel : class
            => self.EmitCall(
                UnselectTextMethod,
                new List<ValueExpression> { ValueExpression.Literal(text) });

        /// <summary>Checks all ListView items when the MVC builder has enabled checkboxes.</summary>
        public static ComponentRef<FusionListView, TModel> CheckAllItems<TModel>(
            this ComponentRef<FusionListView, TModel> self)
            where TModel : class
            => self.EmitCall(CheckAllItemsMethod);

        /// <summary>Unchecks all ListView items when the MVC builder has enabled checkboxes.</summary>
        public static ComponentRef<FusionListView, TModel> UncheckAllItems<TModel>(
            this ComponentRef<FusionListView, TModel> self)
            where TModel : class
            => self.EmitCall(UncheckAllItemsMethod);
    }
}
