namespace DMFS.Models;

public class CowManageViewModel
{
    public List<Cow> Cows { get; set; } = new();
    public Cow Draft { get; set; } = new();
    public List<CowStatus> Statuses { get; set; } = new();
}

public class MilkProductionPageViewModel
{
    public List<MilkProduction> Productions { get; set; } = new();
    public List<Cow> Cows { get; set; } = new();
    public MilkProduction Draft { get; set; } = new();
    public int? FilterCowId { get; set; }
    public DateTime? FilterDate { get; set; }
    public string Search { get; set; } = string.Empty;
}

public class HealthPageViewModel
{
    public List<HealthReport> Reports { get; set; } = new();
    public List<Cow> Cows { get; set; } = new();
    public HealthReport Draft { get; set; } = new();
}

public class BreedPageViewModel
{
    public List<BreedReport> Reports { get; set; } = new();
    public List<Cow> Cows { get; set; } = new();
    public BreedReport Draft { get; set; } = new();
}

public class MilkSalesPageViewModel
{
    public List<MilkSale> Sales { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public MilkSale Draft { get; set; } = new();
}

public class EmployeePageViewModel
{
    public List<Employee> Employees { get; set; } = new();
    public Employee Draft { get; set; } = new();
}
