using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion PivotView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionPivotView(plan, "care-pivot", b =&gt; ...)</c>, reach it with
    /// <c>var pivot = p.Component&lt;FusionPivotView&gt;("care-pivot");</c> and use its members directly:
    /// <c>pivot.DataSource.Set(json.Get(x =&gt; x.Rows)); pivot.Refresh();</c>.
    /// </remarks>
    public sealed class FusionPivotView : FusionComponent
    {
        /// <summary>The shown view: <c>"Table"</c> or <c>"Chart"</c>.</summary>
        public Prop<string> CurrentView => Prop<string>();

        /// <summary>The rows the pivot summarizes; call <see cref="Refresh"/> after writing it.</summary>
        public Prop<object> DataSource => Prop<object>("dataSourceSettings.dataSource");

        /// <summary>Re-renders the pivot from its data source.</summary>
        public void Refresh() => Call();

        /// <summary>The layout as JSON.</summary>
        /// <param name="isRemoveDatasource"><see langword="true"/> to leave the data source out of the JSON.</param>
        public TypedSource<string> GetPersistData(Arg<bool> isRemoveDatasource) => Invoke<string>(isRemoveDatasource);

        /// <summary>Restores a layout from its JSON.</summary>
        /// <param name="persistData">The layout JSON, as <see cref="GetPersistData"/> reads it.</param>
        public void LoadPersistData(Arg<string> persistData) => Call(persistData);

        /// <summary>Opens the conditional formatting dialog.</summary>
        public void ShowConditionalFormattingDialog() => Call();

        /// <summary>Opens the calculated field dialog.</summary>
        public void CreateCalculatedFieldDialog() => Call();

        /// <summary>Raised after the pivot renders its data.</summary>
        public TypedEvent<FusionPivotViewDataBoundArgs> DataBound =>
            Event<FusionPivotViewDataBoundArgs>();

        /// <summary>Raised when the user selects a cell.</summary>
        public TypedEvent<FusionPivotViewCellSelectingArgs> CellSelecting =>
            Event<FusionPivotViewCellSelectingArgs>();
    }

    /// <summary>Payload of <see cref="FusionPivotView.DataBound"/>; carries no data.</summary>
    public class FusionPivotViewDataBoundArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionPivotView.CellSelecting"/>.</summary>
    public class FusionPivotViewCellSelectingArgs : JsObject
    {
        /// <summary>The selected cell.</summary>
        public ReadOnly<FusionPivotViewCellData> Data => ReadOnly<FusionPivotViewCellData>();
    }
#nullable restore annotations

    /// <summary>A pivot cell as the <see cref="FusionPivotView.CellSelecting"/> payload reports it.</summary>
    public class FusionPivotViewCellData
    {
        /// <summary>The axis the cell belongs to.</summary>
        public string Axis { get; set; } = "";

        /// <summary>The cell's field text.</summary>
        public string ActualText { get; set; } = "";

        /// <summary>The cell text as shown.</summary>
        public string FormattedText { get; set; } = "";

        /// <summary>The cell value as shown.</summary>
        public decimal Value { get; set; }

        /// <summary>The cell value before formatting.</summary>
        public decimal ActualValue { get; set; }

        /// <summary>Row index of the cell.</summary>
        public int RowIndex { get; set; }

        /// <summary>Column index of the cell.</summary>
        public int ColIndex { get; set; }

        /// <summary>Row headers of the cell.</summary>
        public string RowHeaders { get; set; } = "";

        /// <summary>Column headers of the cell.</summary>
        public string ColumnHeaders { get; set; } = "";

        /// <summary>Whether the cell is a subtotal.</summary>
        public bool IsSum { get; set; }

        /// <summary>Whether the cell is a grand total.</summary>
        public bool IsGrandSum { get; set; }
    }

    /// <summary>
    /// Factory extension for rendering Syncfusion PivotView with reactive event support.
    /// </summary>
    public static class FusionPivotViewHtmlExtensions
    {
        /// <summary>Renders a Syncfusion PivotView with the element ID the view chooses.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the component's events are wired into.</param>
        /// <param name="elementId">The component's element ID, chosen by the view.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionPivotView, TModel> FusionPivotView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<PivotViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().PivotView(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionPivotView>(elementId, builder.Render());
        }
    }
}
