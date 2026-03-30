namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Configures the label and required marker for an input field wrapper.
    /// </summary>
    /// <remarks>
    /// Passed as an optional callback to <c>Html.InputField(plan, expr, o =&gt; o.Label("Name").Required())</c>.
    /// When no options are provided, the field renders without a label or required indicator.
    /// </remarks>
    public class InputFieldOptions
    {
        /// <summary>Gets the label text, or <see langword="null"/> if no label was configured.</summary>
        internal string? LabelText { get; private set; }

        /// <summary>Gets whether the required marker (<c>*</c>) should be shown.</summary>
        internal bool IsRequired { get; private set; }

        /// <summary>
        /// Marks the field as required, showing a <c>*</c> indicator next to the label.
        /// </summary>
        /// <returns>This options instance for chaining.</returns>
        public InputFieldOptions Required() { IsRequired = true; return this; }

        /// <summary>
        /// Sets the label text displayed above the input component.
        /// </summary>
        /// <param name="label">The label text to display.</param>
        /// <returns>This options instance for chaining.</returns>
        public InputFieldOptions Label(string label) { LabelText = label; return this; }
    }
}
