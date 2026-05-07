namespace DMFS.Models;

public class Employee
{
    public int EmpId { get; set; }
    public string EmpName { get; set; } = string.Empty;
    public DateTime EmpDob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmpPass { get; set; } = string.Empty;

    public ICollection<MilkSale> MilkSales { get; set; } = new List<MilkSale>();
    public ICollection<ExpenditureEntry> ExpenditureEntries { get; set; } = new List<ExpenditureEntry>();
    public ICollection<IncomeEntry> IncomeEntries { get; set; } = new List<IncomeEntry>();
}
