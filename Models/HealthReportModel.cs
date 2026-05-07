namespace DMFS.Models;

public class HealthReport
{
    public int RepId { get; set; }
    public int CowId { get; set; }
    public string CowName { get; set; } = string.Empty;
    public DateTime RepDate { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string VetName { get; set; } = string.Empty;

    public Cow? Cow { get; set; }
}
