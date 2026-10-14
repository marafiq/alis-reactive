namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.OtpInput
{
    /// <summary>
    /// A visitor checking in at the front desk with the 4-digit code texted to them when they booked.
    /// </summary>
    public sealed class OtpInputModel
    {
        /// <summary>The visit code the visitor reads out to the desk.</summary>
        public string? VisitCode { get; set; }
    }

    /// <summary>The code the desk typed, and the code it checked before this one.</summary>
    public sealed class VisitCodeCheck
    {
        /// <summary>The code to check against today's visits.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>The code checked before this one; empty for the first code typed.</summary>
        public string PreviousCode { get; set; } = string.Empty;
    }

    /// <summary>What the desk tells the visitor about the code.</summary>
    public sealed class VisitCodeChecked
    {
        /// <summary>The check-in, or why the code was not accepted.</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>A code the desk issues to a visitor who did not book.</summary>
    public sealed class WalkInCodeIssued
    {
        /// <summary>The 4-digit code that opens the front door today.</summary>
        public string Code { get; set; } = string.Empty;
    }
}
