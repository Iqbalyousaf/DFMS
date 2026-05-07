namespace DMFS.Models;

public class ExpenditureEntry
{
    public int ExpId { get; set; }
    public DateTime ExpDate { get; set; }
    public string ExpPurpose { get; set; } = string.Empty;
    public decimal ExpAmount { get; set; }
    public int EmpId { get; set; }

    public Employee? Employee { get; set; }
}

public class IncomeEntry
{
    public int IncId { get; set; }
    public DateTime IncDate { get; set; }
    public string IncPurpose { get; set; } = string.Empty;
    public decimal IncAmt { get; set; }
    public int EmpId { get; set; }

    public Employee? Employee { get; set; }
}

public class FinanceDashboardModel
{
    public int EmployeeId { get; set; }
    public int? FilterEmployeeId { get; set; }
    public List<Employee> Employees { get; set; } = new();
    public List<ExpenditureEntry> Expenditures { get; set; } = new();
    public List<IncomeEntry> Incomes { get; set; } = new();
}
