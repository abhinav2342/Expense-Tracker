using Expense_tracker.Models;
using Expense_tracker.Models.EntityModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Transactions;

namespace Expense_Tracker.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashBoardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index()
        {
            //last 7 days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            
            List<Expense_tracker.Models.EntityModels.Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();


            //Income
            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            //Expense
            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");


            //Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance =  String.Format(culture, "{0:C0}", Balance);


            //Doughnut - exp category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    fAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .ToList();

            List<LineChartData> IncomeChart = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(K => new LineChartData()
                {
                    day = K.First().Date.ToString("dd-MMM"),
                    income = K.Sum(l => l.Amount)
                })
                .ToList();

            List<LineChartData> ExpenseChart = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(K => new LineChartData()
                {
                    day = K.First().Date.ToString("dd-MMM"),
                    income = K.Sum(l => l.Amount)
                })
                .ToList();

            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.LineChartData = from day in Last7Days
                                    join income in IncomeChart on day equals income.day into dayIncome
                                    from income in dayIncome.DefaultIfEmpty()
                                    join expense in ExpenseChart on day equals expense.day into dayExpense
                                    from expense in dayExpense.DefaultIfEmpty()
                                    select new
                                    {
                                        day = day,
                                        income = income == null ? 0 : income.income,
                                        expense = expense == null ? 0 : expense.expense,
                                    };


            //recent transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }

    public class LineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
