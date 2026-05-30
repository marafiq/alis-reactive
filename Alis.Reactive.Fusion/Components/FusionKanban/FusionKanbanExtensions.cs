using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Runtime behavior for Syncfusion Kanban. Initial board setup remains on
    /// Syncfusion's KanbanBuilder; these members cover post-render reads,
    /// mutations, and method-return sources.
    /// </summary>
    public static class FusionKanbanExtensions
    {
        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod GetColumnDataByStringMethod =
            ComponentMethod.Named("getColumnData").WithArgs<string>();

        private static readonly ComponentMethod GetSwimlaneDataMethod =
            ComponentMethod.Named("getSwimlaneData").WithArgs<string>();

        private static readonly ComponentMethod AddCardAtMethod =
            ComponentMethod.Mapped("addCardAt", "addCard").WithArgs<object, int>();

        private static readonly ComponentMethod UpdateCardMethod =
            ComponentMethod.Named("updateCard").WithArgs<object>();

        private static readonly ComponentMethod DeleteCardMethod =
            ComponentMethod.Named("deleteCard").WithArgs<object>();

        private static readonly ComponentMethod AddColumnMethod =
            ComponentMethod.Named("addColumn").WithArgs<object, int>();

        private static readonly ComponentMethod DeleteColumnMethod =
            ComponentMethod.Named("deleteColumn").WithArgs<int>();

        private static readonly ComponentMethod ShowColumnByStringMethod =
            ComponentMethod.Named("showColumn").WithArgs<string>();

        private static readonly ComponentMethod HideColumnByStringMethod =
            ComponentMethod.Named("hideColumn").WithArgs<string>();

        private static readonly ComponentMethod ShowSpinnerMethod =
            ComponentMethod.Named("showSpinner");

        private static readonly ComponentMethod HideSpinnerMethod =
            ComponentMethod.Named("hideSpinner");

        private static readonly ComponentMethod OpenDialogMethod =
            ComponentMethod.Named("openDialog").WithArgs<string, object>();

        private static readonly ComponentMethod CloseDialogMethod =
            ComponentMethod.Named("closeDialog");

        public static TypedComponentSource<TCard[]> Cards<TModel, TCard>(
            this ComponentRef<FusionKanban, TModel> self)
            where TModel : class
            where TCard : class
            => self.Read(ComponentProperty<TCard[]>.Named("dataSource"));

        public static TypedComponentSource<TCard[]> ColumnData<TModel, TCard>(
            this ComponentRef<FusionKanban, TModel> self,
            string columnKey)
            where TModel : class
            where TCard : class
            => self.Read<TCard[]>(
                GetColumnDataByStringMethod,
                new List<ValueExpression> { ValueExpression.Literal(columnKey) });

        public static TypedComponentSource<TCard[]> SwimlaneData<TModel, TCard>(
            this ComponentRef<FusionKanban, TModel> self,
            string swimlaneKey)
            where TModel : class
            where TCard : class
            => self.Read<TCard[]>(
                GetSwimlaneDataMethod,
                new List<ValueExpression> { ValueExpression.Literal(swimlaneKey) });

        public static ComponentRef<FusionKanban, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionKanban, TModel> self,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
            return self.EmitCall(DataBindMethod);
        }

        /// <summary>
        /// Replaces the board data source with a typed array source — including a client-side
        /// <see cref="Alis.Reactive.Builders.Arrays.ReactiveArray{T}"/> transform via <c>AsSource()</c>.
        /// Routes any card array into the board with no HTTP round-trip, then data-binds.
        /// </summary>
        public static ComponentRef<FusionKanban, TModel> SetDataSource<TModel, TElement>(
            this ComponentRef<FusionKanban, TModel> self,
            TypedSource<TElement[]> source)
            where TModel : class
        {
            self.EmitSet(DataSourceProperty, source.ToValueExpression());
            return self.EmitCall(DataBindMethod);
        }

        public static ComponentRef<FusionKanban, TModel> AddCard<TModel, TResponse>(
            this ComponentRef<FusionKanban, TModel> self,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path,
            int index)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var card = ValueExpression.Read(source.Scope, sourcePath);
            return self.EmitCall(AddCardAtMethod, new List<ValueExpression> { card, ValueExpression.Literal(index) });
        }

        public static ComponentRef<FusionKanban, TModel> UpdateCard<TModel, TResponse>(
            this ComponentRef<FusionKanban, TModel> self,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var card = ValueExpression.Read(source.Scope, sourcePath);
            return self.EmitCall(UpdateCardMethod, new List<ValueExpression> { card });
        }

        public static ComponentRef<FusionKanban, TModel> DeleteCard<TModel, TResponse>(
            this ComponentRef<FusionKanban, TModel> self,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitCall(DeleteCardMethod, new List<ValueExpression> { ValueExpression.Read(source.Scope, sourcePath) });
        }

        public static ComponentRef<FusionKanban, TModel> AddColumn<TModel>(
            this ComponentRef<FusionKanban, TModel> self,
            FusionKanbanColumn column,
            int index)
            where TModel : class
            => self.EmitCall(
                AddColumnMethod,
                new List<ValueExpression>
                {
                    ValueExpression.LiteralRaw(column, Shape.Any),
                    ValueExpression.Literal(index)
                });

        public static ComponentRef<FusionKanban, TModel> DeleteColumn<TModel>(
            this ComponentRef<FusionKanban, TModel> self,
            int index)
            where TModel : class
            => self.EmitCall(DeleteColumnMethod, new List<ValueExpression> { ValueExpression.Literal(index) });

        public static ComponentRef<FusionKanban, TModel> ShowColumn<TModel>(
            this ComponentRef<FusionKanban, TModel> self,
            string key)
            where TModel : class
            => self.EmitCall(ShowColumnByStringMethod, new List<ValueExpression> { ValueExpression.Literal(key) });

        public static ComponentRef<FusionKanban, TModel> HideColumn<TModel>(
            this ComponentRef<FusionKanban, TModel> self,
            string key)
            where TModel : class
            => self.EmitCall(HideColumnByStringMethod, new List<ValueExpression> { ValueExpression.Literal(key) });

        public static ComponentRef<FusionKanban, TModel> ShowSpinner<TModel>(
            this ComponentRef<FusionKanban, TModel> self)
            where TModel : class
            => self.EmitCall(ShowSpinnerMethod);

        public static ComponentRef<FusionKanban, TModel> HideSpinner<TModel>(
            this ComponentRef<FusionKanban, TModel> self)
            where TModel : class
            => self.EmitCall(HideSpinnerMethod);

        public static ComponentRef<FusionKanban, TModel> OpenAddDialog<TModel, TCard>(
            this ComponentRef<FusionKanban, TModel> self,
            TCard card)
            where TModel : class
            where TCard : class
            => self.EmitCall(OpenDialogMethod, DialogArgs("Add", card));

        public static ComponentRef<FusionKanban, TModel> OpenEditDialog<TModel, TCard>(
            this ComponentRef<FusionKanban, TModel> self,
            TCard card)
            where TModel : class
            where TCard : class
            => self.EmitCall(OpenDialogMethod, DialogArgs("Edit", card));

        public static ComponentRef<FusionKanban, TModel> CloseDialog<TModel>(
            this ComponentRef<FusionKanban, TModel> self)
            where TModel : class
            => self.EmitCall(CloseDialogMethod);

        private static List<ValueExpression> DialogArgs<TCard>(string action, TCard card)
            where TCard : class
            => new List<ValueExpression>
            {
                ValueExpression.Literal(action),
                ValueExpression.LiteralRaw(card, Shape.Any)
            };
    }

    public sealed class FusionKanbanColumn
    {
        public string HeaderText { get; set; } = "";
        public string KeyField { get; set; } = "";
        public bool AllowToggle { get; set; }
        public bool IsExpanded { get; set; } = true;
    }
}
