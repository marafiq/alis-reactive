using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ChipList, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionChipList(plan, "diet-chips", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionChipList&gt;("diet-chips")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. The JavaScript <c>select</c> and <c>remove</c> take several argument types; each is its own
    /// member: <c>.Call(c =&gt; c.SelectByIndexes, new[] { 0, 2 })</c>, <c>.Call(c =&gt; c.SelectByText, new[] { "Diabetic" })</c>.
    /// </remarks>
    public sealed class FusionChipList : FusionComponent
    {
        /// <summary>Adds a chip with this text.</summary>
        public ComponentMethod<string> Add => Method<string>("add");

        /// <summary>Selects the chip at a zero-based index.</summary>
        public ComponentMethod<int> SelectByIndex => Method<int>("select");

        /// <summary>Selects the chips at these zero-based indexes.</summary>
        public ComponentMethod<int[]> SelectByIndexes => Method<int[]>("select");

        /// <summary>Selects the chips with these texts.</summary>
        public ComponentMethod<string[]> SelectByText => Method<string[]>("select").Passing("text");

        /// <summary>Removes the chip at a zero-based index.</summary>
        public ComponentMethod<int> RemoveByIndex => Method<int>("remove");

        /// <summary>Removes the chips at these zero-based indexes.</summary>
        public ComponentMethod<int[]> RemoveByIndexes => Method<int[]>("remove");

        /// <summary>The chip at a zero-based index.</summary>
        public ComponentFunction<int, FusionChipData> Find => Function<int, FusionChipData>("find");

        /// <summary>The selected chips.</summary>
        public ComponentFunction<FusionSelectedChips> GetSelectedChips => Function<FusionSelectedChips>("getSelectedChips");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>The selected chips, by value.</summary>
        public ComponentProperty<string[]> SelectedChipValues => Property<string[]>("selectedChips");

        /// <summary>The selected chips, by zero-based index.</summary>
        public ComponentProperty<int[]> SelectedChipIndexes => Property<int[]>("selectedChips");

        /// <summary>Raised when the user clicks a chip.</summary>
        public TypedEvent<FusionChipListClickArgs> Click =>
            Event<FusionChipListClickArgs>("click");

        /// <summary>Raised after the user deletes a chip.</summary>
        public TypedEvent<FusionChipListDeletedArgs> Deleted =>
            Event<FusionChipListDeletedArgs>("deleted");
    }

    /// <summary>Payload of <see cref="FusionChipList.Click"/>.</summary>
    public class FusionChipListClickArgs
    {
        /// <summary>Text of the clicked chip.</summary>
        public string Text { get; set; } = "";

        /// <summary>Zero-based index of the clicked chip.</summary>
        public int Index { get; set; }

        /// <summary>Whether the chip is selected after the click.</summary>
        public bool Selected { get; set; }
    }

    /// <summary>Payload of <see cref="FusionChipList.Deleted"/>.</summary>
    public class FusionChipListDeletedArgs
    {
        /// <summary>Text of the deleted chip.</summary>
        public string Text { get; set; } = "";

        /// <summary>Zero-based index the chip had.</summary>
        public int Index { get; set; }
    }

    /// <summary>A chip as <see cref="FusionChipList.Find"/> returns it.</summary>
    public class FusionChipData
    {
        /// <summary>The chip text.</summary>
        public string Text { get; set; } = "";

        /// <summary>The chip's zero-based index.</summary>
        public int Index { get; set; }
    }

    /// <summary>A selected chip's data as <see cref="FusionSelectedChips"/> reports it.</summary>
    public class FusionChipItem
    {
        /// <summary>The chip text.</summary>
        public string Text { get; set; } = "";

        /// <summary>The chip value.</summary>
        public string Value { get; set; } = "";
    }

    /// <summary>The selected chips as <see cref="FusionChipList.GetSelectedChips"/> returns them.</summary>
    public class FusionSelectedChips
    {
        /// <summary>Texts of the selected chips.</summary>
        public string[] Texts { get; set; } = Array.Empty<string>();

        /// <summary>Data of the selected chips.</summary>
        public FusionChipItem[] Data { get; set; } = Array.Empty<FusionChipItem>();

        /// <summary>Zero-based indexes of the selected chips.</summary>
        public int[] Indexes { get; set; } = Array.Empty<int>();
    }

    public static class FusionChipListHtmlExtensions
    {
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionChipList, TModel> FusionChipList<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ChipListBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ChipList(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionChipList>(elementId, builder.Render());
        }
    }
}
