namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The admissions desk: the choices its editors offer. The residents' records and the referrals waiting
    /// to move in live in its two grids.
    /// </summary>
    public class AdmissionsDeskViewModel
    {
        public IReadOnlyList<string> CareLevels { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> RiskLevels { get; set; } = Array.Empty<string>();

        /// <summary>The nurses on staff, each with the shift they work.</summary>
        public IReadOnlyList<AdmissionsNurse> Nurses { get; set; } = Array.Empty<AdmissionsNurse>();
    }

    /// <summary>A nurse on staff and the shift they work: <c>days</c>, <c>evenings</c> or <c>nights</c>.</summary>
    public sealed record AdmissionsNurse(string Name, string Shift);

    /// <summary>A resident's record: care level, primary nurse and next care review.</summary>
    public class AdmissionsResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";

        /// <summary>The day of the next care review, as the date editor writes it: <c>2026-10-16</c>.</summary>
        public string NextReview { get; set; } = "";
    }

    /// <summary>An approved referral waiting to move in, completed on the admission form.</summary>
    public class AdmissionsReferral
    {
        public int ReferralId { get; set; }
        public string ResidentName { get; set; } = "";
        public string MoveInDay { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }

        /// <summary>The day of the first care review after moving in: <c>2026-10-26</c>.</summary>
        public string FirstReview { get; set; } = "";
    }

    /// <summary>What the desk holds: the residents by name, and the referrals by the day they move in.</summary>
    public class AdmissionsDesk
    {
        public List<AdmissionsResident> Residents { get; set; } = new();
        public List<AdmissionsReferral> Referrals { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident's record as the coordinator corrected it.</summary>
    public class ResidentRecordCorrection
    {
        public AdmissionsResident Resident { get; set; } = new();
    }

    /// <summary>A referral as the coordinator completed it on the admission form.</summary>
    public class ReferralAdmission
    {
        public AdmissionsReferral Referral { get; set; } = new();
    }

    /// <summary>What the desk did, as the coordinator is told.</summary>
    public class AdmissionsAnswer
    {
        public string Summary { get; set; } = "";
    }
}
