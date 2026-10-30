namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SplitButton
{
    /// <summary>
    /// A care note a coordinator writes for Margaret Hale, signs, and saves to her chart, telling her family or
    /// the nurse when it matters.
    /// </summary>
    public sealed class FusionSplitButtonModel
    {
        /// <summary>What the note is about: daily care, a fall, or a medication change.</summary>
        public string? Kind { get; set; }

        /// <summary>What happened, in the coordinator's words.</summary>
        public string? Note { get; set; }

        /// <summary>The coordinator gave or saw the care themselves.</summary>
        public bool Signed { get; set; }
    }

    /// <summary>A note to save, and what the chosen save does besides.</summary>
    public sealed class CareNoteSave
    {
        /// <summary>What the note is about.</summary>
        public string? Kind { get; set; }

        /// <summary>What happened.</summary>
        public string? Note { get; set; }

        /// <summary>The save menu item's ID, or <c>save</c> for the plain save.</summary>
        public string Action { get; set; } = string.Empty;
    }

    /// <summary>What the chart did with the note.</summary>
    public sealed class CareNoteSaved
    {
        /// <summary>Whether the note is on the chart.</summary>
        public bool Saved { get; set; }

        /// <summary>What happened to the note, or why it was not saved.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>The save menu item IDs whose follow-up is done for this note.</summary>
        public string[] FinishedActions { get; set; } = [];
    }
}
