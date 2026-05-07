namespace DMFS.Models;

public class MilkSale
{
    public int SId { get; set; }
    public DateTime Date { get; set; }
    public decimal Uprice { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public int EmpId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount => Uprice * Quantity;

    public Employee? Employee { get; set; }
}
