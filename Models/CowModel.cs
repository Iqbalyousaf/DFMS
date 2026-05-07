namespace DMFS.Models;

using System.ComponentModel.DataAnnotations;

public class Cow
{
    public int CowId { get; set; }

    [Required(ErrorMessage = "Cow name is required.")]
    public string CowName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ear tag is required.")]
    public string EarTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Color is required.")]
    public string Color { get; set; } = string.Empty;

    [Required(ErrorMessage = "Breed is required.")]
    public string Breed { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Age must be between {1} and {2}.")]
    public int Age { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [DataType(DataType.Date)]
    public DateTime? InactiveDate { get; set; }

    [Range(0.01, 1000, ErrorMessage = "Weight at birth must be greater than 0.")]
    public double WeightAtBirth { get; set; }

    [Required(ErrorMessage = "Pasture is required.")]
    public string Pasture { get; set; } = string.Empty;

    // Soft-delete / status
    public bool IsActive { get; set; } = true;
    // Reason for inactive status (optional)
    public string? InactiveReason { get; set; }

    public ICollection<MilkProduction> MilkProductions { get; set; } = new List<MilkProduction>();
    public ICollection<HealthReport> HealthReports { get; set; } = new List<HealthReport>();
    public ICollection<BreedReport> BreedReports { get; set; } = new List<BreedReport>();
}
