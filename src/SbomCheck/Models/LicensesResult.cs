namespace SbomCheck.Models
{
    public class LicensesResult
    {
        public LicensesStatus Status { get; set; }
        public int TotalComponents { get; set; }
        public List<LicenseDetail> LicenseDetails { get; set; } = [];
    }
}
