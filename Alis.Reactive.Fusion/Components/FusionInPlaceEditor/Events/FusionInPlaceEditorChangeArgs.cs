namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered when the inner editor's value changes.</summary>
    /// <remarks>
    /// Fires on inner-editor value changes during the edit, including user-initiated changes before
    /// commit. For a commit hook use <c>ActionSuccess</c>.
    /// </remarks>
    public class FusionInPlaceEditorChangeArgs
    {
        /// <summary>Inner editor value reported by the change event, surfaced as a string.</summary>
        public string? Value { get; set; }

        /// <summary>Inner editor value before this change event.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Syncfusion event token exposed as <c>args.name</c>.</summary>
        public string? Name { get; set; }
    }
}
