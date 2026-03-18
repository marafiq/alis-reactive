namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A single radio button option — value, display text, and optional description.
    /// Controller creates these, builder consumes them via .Items().
    /// </summary>
    public class RadioButtonItem
    {
        public string Value { get; }
        public string Text { get; }
        public string? Description { get; }

        public RadioButtonItem(string value, string text, string? description = null)
        {
            Value = value;
            Text = text;
            Description = description;
        }
    }
}
