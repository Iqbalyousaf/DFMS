using DMFS.Data;
using DMFS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Home()
    {
        var totalIncome = await _context.IncomeEntries.SumAsync(i => (decimal?)i.IncAmt) ?? 0;
        var totalExp = await _context.ExpenditureEntries.SumAsync(e => (decimal?)e.ExpAmount) ?? 0;
        var totalProduced = await _context.MilkProductions.SumAsync(m =>
            (double?)(m.AmMilk + m.PmMilk)) ?? 0;
        var totalSold = (double)(await _context.MilkSales.SumAsync(s => (decimal?)s.Quantity) ?? 0m);
        var milkLiters = Math.Max(0, totalProduced - totalSold);

        var topSale = await _context.MilkSales
            .OrderByDescending(s => s.Uprice * s.Quantity)
            .Select(s => new { Amt = s.Uprice * s.Quantity, s.Date })
            .FirstOrDefaultAsync();

        var topExp = await _context.ExpenditureEntries
            .OrderByDescending(e => e.ExpAmount)
            .Select(e => new { e.ExpAmount, e.ExpDate })
            .FirstOrDefaultAsync();

        var vm = new DashboardViewModel
        {
            TotalIncome = totalIncome,
            TotalExpenditure = totalExp,
            Balance = totalIncome - totalExp,
            CowCount = await _context.Cows.CountAsync(),
            MilkStockLiters = milkLiters,
            EmployeeCount = await _context.Employees.CountAsync(),
            HighestSaleAmount = topSale?.Amt,
            HighestSaleDate = topSale?.Date,
            HighestExpenditureAmount = topExp?.ExpAmount,
            HighestExpenditureDate = topExp?.ExpDate
        };

        return View("~/Views/Dashboard/Home.cshtml", vm);
    }
}
