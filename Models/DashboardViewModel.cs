namespace DMFS.Models;

public class DashboardViewModel
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenditure { get; set; }
    public decimal Balance { get; set; }
    public int CowCount { get; set; }
    public double MilkStockLiters { get; set; }
    public int EmployeeCount { get; set; }
    public decimal? HighestSaleAmount { get; set; }
    public DateTime? HighestSaleDate { get; set; }
    public decimal? HighestExpenditureAmount { get; set; }
    public DateTime? HighestExpenditureDate { get; set; }
}
