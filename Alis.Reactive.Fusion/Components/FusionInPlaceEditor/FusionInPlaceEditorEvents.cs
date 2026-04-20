namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionInPlaceEditor"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.ActionSuccess, (args, p) =&gt; { … })</c>.
    /// <para>
    /// Commit flow (<c>url</c> not set): <c>beginEdit → change → endEdit → actionBegin → actionSuccess → submitClick</c>.
    /// Hook <see cref="ActionSuccess"/> for the commit POST — it only fires after validation passes.
    /// <see cref="SubmitClick"/> fires on every user save intent including blocked saves and is not a commit-success signal.
    /// </para>
    /// </remarks>
    public sealed class FusionInPlaceEditorEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionInPlaceEditorEvents Instance = new FusionInPlaceEditorEvents();
        private FusionInPlaceEditorEvents() { }

        /// <summary>Fires before the editor enters edit mode (SF "beginEdit" event).</summary>
        public TypedEvent<FusionInPlaceEditorBeginEditArgs> BeginEdit =>
            new TypedEvent<FusionInPlaceEditorBeginEditArgs>(
                "beginEdit", new FusionInPlaceEditorBeginEditArgs());

        /// <summary>Fires when the editor leaves edit mode (SF "endEdit" event).</summary>
        public TypedEvent<FusionInPlaceEditorEndEditArgs> EndEdit =>
            new TypedEvent<FusionInPlaceEditorEndEditArgs>(
                "endEdit", new FusionInPlaceEditorEndEditArgs());

        /// <summary>Fires when the inner editor's value changes (SF "change" event).</summary>
        public TypedEvent<FusionInPlaceEditorChangeArgs> Changed =>
            new TypedEvent<FusionInPlaceEditorChangeArgs>(
                "change", new FusionInPlaceEditorChangeArgs());

        /// <summary>Fires during validation (SF "validating" event). Only fires when builder configures <c>validationRules</c>.</summary>
        public TypedEvent<FusionInPlaceEditorValidatingArgs> Validating =>
            new TypedEvent<FusionInPlaceEditorValidatingArgs>(
                "validating", new FusionInPlaceEditorValidatingArgs());

        /// <summary>Fires before the submit step (SF "actionBegin" event). Rarely hooked; set <c>cancel = true</c> via <c>PreventDefault</c> to block.</summary>
        public TypedEvent<FusionInPlaceEditorActionBeginArgs> ActionBegin =>
            new TypedEvent<FusionInPlaceEditorActionBeginArgs>(
                "actionBegin", new FusionInPlaceEditorActionBeginArgs());

        /// <summary>Fires after a successful commit (SF "actionSuccess"). Primary hook for the reactive POST: only fires after validation passes.</summary>
        public TypedEvent<FusionInPlaceEditorActionSuccessArgs> ActionSuccess =>
            new TypedEvent<FusionInPlaceEditorActionSuccessArgs>(
                "actionSuccess", new FusionInPlaceEditorActionSuccessArgs());

        /// <summary>Fires after a failed SF-initiated commit (SF "actionFailure"). Only meaningful if the builder configures a URL; otherwise this path is unused.</summary>
        public TypedEvent<FusionInPlaceEditorActionFailureArgs> ActionFailure =>
            new TypedEvent<FusionInPlaceEditorActionFailureArgs>(
                "actionFailure", new FusionInPlaceEditorActionFailureArgs());

        /// <summary>Fires on a user save click or Enter key (SF "submitClick"). Fires even when validation blocks the commit; use <see cref="ActionSuccess"/> for post-commit side effects.</summary>
        public TypedEvent<FusionInPlaceEditorSubmitClickArgs> SubmitClick =>
            new TypedEvent<FusionInPlaceEditorSubmitClickArgs>(
                "submitClick", new FusionInPlaceEditorSubmitClickArgs());

        /// <summary>Fires on a user cancel click (SF "cancelClick").</summary>
        public TypedEvent<FusionInPlaceEditorCancelClickArgs> CancelClick =>
            new TypedEvent<FusionInPlaceEditorCancelClickArgs>(
                "cancelClick", new FusionInPlaceEditorCancelClickArgs());
    }
}
