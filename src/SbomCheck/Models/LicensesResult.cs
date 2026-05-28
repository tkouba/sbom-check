namespace SbomCheck.Models
{
    public class LicensesResult
    {
        public LicenseStatus Status { get; set; }
        public int TotalComponents { get; set; }
        public List<LicenseDetail> LicenseDetails { get; set; } = [];
        public List<ComponentRuleViolation> ComponentViolations { get; set; } = [];
        public List<IgnoredComponentInfo> IgnoredComponents { get; set; } = [];
    }
}
