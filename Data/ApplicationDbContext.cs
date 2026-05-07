using DMFS.Models;
using Microsoft.EntityFrameworkCore;

namespace DMFS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cow> Cows => Set<Cow>();
        public DbSet<CowStatus> CowStatuses => Set<CowStatus>();
        public DbSet<MilkProduction> MilkProductions => Set<MilkProduction>();
        public DbSet<HealthReport> HealthReports => Set<HealthReport>();
        public DbSet<BreedReport> BreedReports => Set<BreedReport>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<MilkSale> MilkSales => Set<MilkSale>();
        public DbSet<ExpenditureEntry> ExpenditureEntries => Set<ExpenditureEntry>();
        public DbSet<IncomeEntry> IncomeEntries => Set<IncomeEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MilkProduction>().HasKey(x => x.MId);
            modelBuilder.Entity<HealthReport>().HasKey(x => x.RepId);
            modelBuilder.Entity<BreedReport>().HasKey(x => x.Brid);
            modelBuilder.Entity<Employee>().HasKey(x => x.EmpId);
            modelBuilder.Entity<MilkSale>().HasKey(x => x.SId);
            modelBuilder.Entity<ExpenditureEntry>().HasKey(x => x.ExpId);
            modelBuilder.Entity<IncomeEntry>().HasKey(x => x.IncId);

            modelBuilder.Entity<MilkProduction>().Ignore(x => x.TotalMilk);
            modelBuilder.Entity<MilkSale>().Ignore(x => x.Amount);

            modelBuilder.Entity<ExpenditureEntry>().Property(x => x.ExpAmount).HasPrecision(18, 2);
            modelBuilder.Entity<HealthReport>().Property(x => x.Cost).HasPrecision(18, 2);
            modelBuilder.Entity<IncomeEntry>().Property(x => x.IncAmt).HasPrecision(18, 2);
            modelBuilder.Entity<MilkSale>().Property(x => x.Uprice).HasPrecision(18, 2);
            modelBuilder.Entity<MilkSale>().Property(x => x.Quantity).HasPrecision(18, 2);

            modelBuilder.Entity<Cow>()
                .HasMany(c => c.MilkProductions)
                .WithOne(m => m.Cow)
                .HasForeignKey(m => m.CowId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cow>()
                .HasMany(c => c.HealthReports)
                .WithOne(h => h.Cow)
                .HasForeignKey(h => h.CowId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cow>()
                .HasMany(c => c.BreedReports)
                .WithOne(b => b.Cow)
                .HasForeignKey(b => b.CowId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.MilkSales)
                .WithOne(s => s.Employee)
                .HasForeignKey(s => s.EmpId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.ExpenditureEntries)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmpId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.IncomeEntries)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmpId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
