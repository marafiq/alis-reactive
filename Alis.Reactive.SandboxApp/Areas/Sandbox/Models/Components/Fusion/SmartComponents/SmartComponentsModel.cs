namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SmartComponents
{
    /// <summary>
    /// An admissions coordinator's referral intake: the referral opened from the inbox or pasted as a letter,
    /// the form the letter fills in, and the coordinator's own intake note.
    /// </summary>
    public sealed class SmartComponentsModel
    {
        /// <summary>The referral opened from today's inbox.</summary>
        public string? IncomingReferral { get; set; }

        /// <summary>The resident the referral is for.</summary>
        public string? ResidentName { get; set; }

        /// <summary>The physician who referred them.</summary>
        public string? ReferredBy { get; set; }

        /// <summary>The level of care the referral asks for.</summary>
        public string? CareLevel { get; set; }

        /// <summary>Why the resident is referred.</summary>
        public string? Reason { get; set; }

        /// <summary>The coordinator's intake note; <see langword="null"/> when none is written, as the smart text area holds no value until typed in.</summary>
        public string? IntakeNote { get; set; }

        /// <summary>Today's referrals in the inbox, by ID and the line the inbox shows.</summary>
        public IReadOnlyList<IncomingReferral> Inbox { get; set; } = [];

        /// <summary>The levels of care the community offers.</summary>
        public IReadOnlyList<string> CareLevels { get; set; } = [];
    }

    /// <summary>A referral in today's inbox.</summary>
    public sealed class IncomingReferral
    {
        /// <summary>The referral's ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Who sent it, and who it is for.</summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>The referral whose letter is opened.</summary>
    public sealed class ReferralLetterQuery
    {
        /// <summary>The referral's ID.</summary>
        public string ReferralId { get; set; } = string.Empty;
    }

    /// <summary>The referral the coordinator saves.</summary>
    public sealed class ReferralIntakeSave
    {
        /// <summary>The resident the referral is for.</summary>
        public string ResidentName { get; set; } = string.Empty;

        /// <summary>The level of care the referral asks for.</summary>
        public string CareLevel { get; set; } = string.Empty;

        /// <summary>The coordinator's intake note; <see langword="null"/> when none is written, as the smart text area holds no value until typed in.</summary>
        public string? IntakeNote { get; set; }
    }

    /// <summary>What the coordinator is told about the saved referral.</summary>
    public sealed class ReferralIntakeSaved
    {
        /// <summary>The referral saved, with the length of its intake note.</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>The chat request Syncfusion's smart components send their AI service.</summary>
    public sealed class SmartAssistRequest
    {
        /// <summary>The conversation: the instructions, then the user's text.</summary>
        public IReadOnlyList<SmartAssistMessage> Messages { get; set; } = [];
    }

    /// <summary>One message of a <see cref="SmartAssistRequest"/>.</summary>
    public sealed class SmartAssistMessage
    {
        /// <summary>"system", "user" or "assistant".</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>The message's text.</summary>
        public string Content { get; set; } = string.Empty;
    }
}
