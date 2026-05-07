using DMFS.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Force application culture to English (en-US) so validation messages and dialogs appear in English
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    // Safety net: if migration history is out-of-sync, ensure required cow status columns exist.
    dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Cows', 'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cows] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_Cows_IsActive] DEFAULT(1);
END
IF COL_LENGTH('dbo.Cows', 'InactiveReason') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cows] ADD [InactiveReason] nvarchar(max) NULL;
END
IF COL_LENGTH('dbo.Cows', 'InactiveDate') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cows] ADD [InactiveDate] datetime2 NULL;
END");
    dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.MilkProductions', 'Status') IS NULL
BEGIN
    ALTER TABLE [dbo].[MilkProductions] ADD [Status] nvarchar(max) NOT NULL CONSTRAINT [DF_MilkProductions_Status] DEFAULT('Draft');
END");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Home}/{id?}");

app.Run();
