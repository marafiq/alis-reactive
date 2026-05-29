using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionChipListExtensions
    {
        private static readonly ComponentMethod AddMethod =
            ComponentMethod.Named("add").WithArgs<string>();

        private static readonly ComponentMethod SelectByIndexMethod =
            ComponentMethod.Mapped("selectByIndex", "select").WithArgs<int>();

        private static readonly ComponentMethod SelectByIndexesMethod =
            ComponentMethod.Mapped("selectByIndexes", "select").WithArgs<int[]>();

        private static readonly ComponentMethod SelectByTextMethod =
            ComponentMethod.Mapped("selectByText", "select").WithArgs<string[], string>();

        private static readonly ComponentMethod RemoveMethod =
            ComponentMethod.Named("remove").WithArgs<int>();

        private static readonly ComponentMethod RemoveIndexesMethod =
            ComponentMethod.Mapped("removeIndexes", "remove").WithArgs<int[]>();

        private static readonly ComponentMethod FindMethod =
            ComponentMethod.Named("find").WithArgs<int>();

        private static readonly ComponentMethod GetSelectedChipsMethod =
            ComponentMethod.Named("getSelectedChips");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentProperty<string[]> SelectedChipValuesProperty =
            ComponentProperty<string[]>.Mapped("selectedChipValues", "selectedChips");

        private static readonly ComponentProperty<int[]> SelectedChipIndexesProperty =
            ComponentProperty<int[]>.Mapped("selectedChipIndexes", "selectedChips");

        public static ComponentRef<FusionChipList, TModel> Add<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            string text)
            where TModel : class
            => self.EmitCall(
                AddMethod,
                new List<ValueExpression> { ValueExpression.Literal(text) });

        public static ComponentRef<FusionChipList, TModel> SelectByIndex<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            int index)
            where TModel : class
            => self.EmitCall(
                SelectByIndexMethod,
                new List<ValueExpression> { ValueExpression.Literal(index) });

        public static ComponentRef<FusionChipList, TModel> SelectByIndexes<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            params int[] indexes)
            where TModel : class
            => self.EmitCall(
                SelectByIndexesMethod,
                new List<ValueExpression>
                {
                    ValueExpression.LiteralRaw(indexes, Shape.ArrayOf(Shape.Number))
                });

        public static ComponentRef<FusionChipList, TModel> SelectByText<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            params string[] text)
            where TModel : class
            => self.EmitCall(
                SelectByTextMethod,
                new List<ValueExpression>
                {
                    ValueExpression.LiteralRaw(text, Shape.ArrayOf(Shape.String)),
                    ValueExpression.Literal("text")
                });

        public static ComponentRef<FusionChipList, TModel> RemoveByIndex<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            int index)
            where TModel : class
            => self.EmitCall(
                RemoveMethod,
                new List<ValueExpression> { ValueExpression.Literal(index) });

        public static ComponentRef<FusionChipList, TModel> RemoveByIndexes<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            params int[] indexes)
            where TModel : class
            => self.EmitCall(
                RemoveIndexesMethod,
                new List<ValueExpression>
                {
                    ValueExpression.LiteralRaw(indexes, Shape.ArrayOf(Shape.Number))
                });

        public static TypedComponentSource<FusionChipData> Find<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            int index)
            where TModel : class
            => self.Read<FusionChipData>(
                FindMethod,
                new List<ValueExpression> { ValueExpression.Literal(index) });

        public static TypedComponentSource<FusionSelectedChips> SelectedChips<TModel>(
            this ComponentRef<FusionChipList, TModel> self)
            where TModel : class
            => self.Read<FusionSelectedChips>(GetSelectedChipsMethod);

        public static TypedComponentSource<string[]> SelectedChipValues<TModel>(
            this ComponentRef<FusionChipList, TModel> self)
            where TModel : class
            => self.Read(SelectedChipValuesProperty);

        public static TypedComponentSource<int[]> SelectedChipIndexes<TModel>(
            this ComponentRef<FusionChipList, TModel> self)
            where TModel : class
            => self.Read(SelectedChipIndexesProperty);

        public static ComponentRef<FusionChipList, TModel> SetSelectedChipIndexes<TModel>(
            this ComponentRef<FusionChipList, TModel> self,
            params int[] indexes)
            where TModel : class
            => self
                .EmitSet(
                    SelectedChipIndexesProperty,
                    ValueExpression.LiteralRaw(indexes, Shape.ArrayOf(Shape.Number)))
                .EmitCall(DataBindMethod);
    }
}
