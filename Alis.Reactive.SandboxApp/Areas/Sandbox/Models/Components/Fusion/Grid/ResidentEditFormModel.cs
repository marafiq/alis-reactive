namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class AdmissionsDeskViewModel
    {
        public IReadOnlyList<string> CareLevels { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> RiskLevels { get; set; } = Array.Empty<string>();

        public IReadOnlyList<AdmissionsNurse> Nurses { get; set; } = Array.Empty<AdmissionsNurse>();
    }

    public sealed record AdmissionsNurse(string Name, string Shift);

    public class AdmissionsResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";

        public string NextReview { get; set; } = "";
    }

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

        public string FirstReview { get; set; } = "";
    }

    public class AdmissionsDesk
    {
        public List<AdmissionsResident> Residents { get; set; } = new();
        public List<AdmissionsReferral> Referrals { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class ResidentRecordCorrection
    {
        public AdmissionsResident Resident { get; set; } = new();
    }

    public class ReferralAdmission
    {
        public AdmissionsReferral Referral { get; set; } = new();
    }

    public class AdmissionsAnswer
    {
        public string Summary { get; set; } = "";
    }
}
