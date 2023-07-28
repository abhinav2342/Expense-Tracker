using Expense_tracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

CultureInfo info = new CultureInfo("en-GB");
info.DateTimeFormat.ShortDatePattern = "dd MMM yyyy";
info.DateTimeFormat.LongDatePattern = "dd MMM yyyy HH:mm";
info.NumberFormat.CurrencyDecimalDigits = 2;
info.NumberFormat.CurrencyGroupSeparator = ",";
info.NumberFormat.NumberDecimalDigits = 2;
Thread.CurrentThread.CurrentCulture = info;
Thread.CurrentThread.CurrentUICulture = info;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

var app = builder.Build();

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
    pattern: "{controller=DashBoard}/{action=Index}/{id?}");

app.Run();
