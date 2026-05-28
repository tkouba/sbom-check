namespace SbomCheck.Models
{
    public enum LicenseStatus
    {
        /// <summary>
        /// Default value, licenses status not resolved
        /// </summary>
        None = 0,
        /// <summary>
        /// License status resolved but contains unknown license(s)
        /// </summary>
        /// <remarks>Reserved for future use, currently treated as Invalid</remarks>
        Unknown,
        /// <summary>
        /// All licenses are valid
        /// </summary>
        Valid,
        /// <summary>
        /// Contains invalid license(s)
        /// </summary>
        Invalid
    }
}
