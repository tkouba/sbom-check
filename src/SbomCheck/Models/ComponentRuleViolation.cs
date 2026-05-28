namespace SbomCheck.Models;

public class ComponentRuleViolation
{
    public string Display { get; set; } = null!;
    public List<ComponentViolation> Components { get; set; } = [];
}
