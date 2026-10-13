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
    /// Render it with <c>Html.FusionChipList(plan, "diet-chips", b =&gt; ...)</c>, then reach it in a callback and
    /// use its members directly. The JavaScript <c>select</c> and <c>remove</c> take several argument types; each
    /// is its own member:
    /// <code>
    /// var chips = p.Component&lt;FusionChipList&gt;("diet-chips");
    /// chips.SelectByIndexes(new[] { 0, 2 });
    /// chips.SelectByText(new[] { "Diabetic" });
    /// </code>
    /// </remarks>
    public sealed class FusionChipList : FusionComponent
    {
        /// <summary>Adds a chip with this text.</summary>
        /// <param name="text">The new chip's text.</param>
        public void Add(Arg<string> text) => Call(text);

        /// <summary>Selects the chip at a zero-based index.</summary>
        /// <param name="index">The chip's zero-based index.</param>
        public void SelectByIndex(Arg<int> index) => Call(index, name: "select");

        /// <summary>Selects the chips at these zero-based indexes.</summary>
        /// <param name="indexes">The chips' zero-based indexes.</param>
        public void SelectByIndexes(Arg<int[]> indexes) => Call(indexes, name: "select");

        /// <summary>Selects the chips with these texts.</summary>
        /// <param name="texts">The texts of the chips to select.</param>
        public void SelectByText(Arg<string[]> texts) => Call(texts, Fixed("text"), name: "select");

        /// <summary>Removes the chip at a zero-based index.</summary>
        /// <param name="index">The chip's zero-based index.</param>
        public void RemoveByIndex(Arg<int> index) => Call(index, name: "remove");

        /// <summary>Removes the chips at these zero-based indexes.</summary>
        /// <param name="indexes">The chips' zero-based indexes.</param>
        public void RemoveByIndexes(Arg<int[]> indexes) => Call(indexes, name: "remove");

        /// <summary>The chip at a zero-based index.</summary>
        /// <param name="index">The chip's zero-based index.</param>
        public TypedSource<FusionChipData> Find(Arg<int> index) => Invoke<FusionChipData>(index);

        /// <summary>The selected chips.</summary>
        public TypedSource<FusionSelectedChips> GetSelectedChips() => Invoke<FusionSelectedChips>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>The selected chips, by value.</summary>
        public Prop<string[]> SelectedChipValues => Prop<string[]>("selectedChips");

        /// <summary>The selected chips, by zero-based index.</summary>
        public Prop<int[]> SelectedChipIndexes => Prop<int[]>("selectedChips");

        /// <summary>Raised when the user clicks a chip.</summary>
        public TypedEvent<FusionChipListClickArgs> Click =>
            Event<FusionChipListClickArgs>();

        /// <summary>Raised after the user deletes a chip.</summary>
        public TypedEvent<FusionChipListDeletedArgs> Deleted =>
            Event<FusionChipListDeletedArgs>();
    }

    /// <summary>Payload of <see cref="FusionChipList.Click"/>.</summary>
    public class FusionChipListClickArgs : JsObject
    {
        /// <summary>Text of the clicked chip.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Zero-based index of the clicked chip.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Whether the chip is selected after the click.</summary>
        public ReadOnly<bool> Selected => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionChipList.Deleted"/>.</summary>
    public class FusionChipListDeletedArgs : JsObject
    {
        /// <summary>Text of the deleted chip.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Zero-based index the chip had.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();
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

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionChipListHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ChipList and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving chip list event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
