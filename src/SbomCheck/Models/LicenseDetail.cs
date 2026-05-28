namespace SbomCheck.Models
{
    public class LicenseDetail
    {
        public string LicenseId { get; set; } = null!;
        public int Count { get; set; }
        public LicenseStatus Status { get; set; }
        public LicenseComponent[] Components { get; set; } = [];
    }
}
