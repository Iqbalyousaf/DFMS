using System.ComponentModel.DataAnnotations;

namespace DMFS.Models
{
    public class MilkProduction
    {
        public int MId { get; set; }

        [Required(ErrorMessage = "Cow selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Cow selection is required.")]
        public int CowId { get; set; }

        public string CowName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Production date is required.")]
        [DataType(DataType.Date)]
        public DateTime DateProd { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "AM milk cannot be negative.")]
        public double AmMilk { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Noon milk cannot be negative.")]
        public double NoonMilk { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "PM milk cannot be negative.")]
        public double PmMilk { get; set; }

        [Required]
        public string Status { get; set; } = "Draft"; // Draft | Approved

        public double TotalMilk => AmMilk + NoonMilk + PmMilk;

        public Cow? Cow { get; set; }
    }
}
