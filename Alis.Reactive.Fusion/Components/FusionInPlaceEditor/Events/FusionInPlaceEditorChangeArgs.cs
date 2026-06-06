namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered when the inner editor's value changes.</summary>
    /// <remarks>
    /// Fires on inner-editor value changes during the edit, including user-initiated changes before
    /// commit. For a commit hook use <c>ActionSuccess</c>.
    /// </remarks>
    public class FusionInPlaceEditorChangeArgs
    {
        /// <summary>The current value of the inner integrated component (surfaced as string).</summary>
        public string? Value { get; set; }

        /// <summary>The previous value of the inner integrated component.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Syncfusion event token exposed as <c>args.name</c>.</summary>
        public string? Name { get; set; }
    }
}
