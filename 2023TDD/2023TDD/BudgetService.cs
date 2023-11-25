#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace _2023TDD;

public class BudgetService
{
    private readonly IBudgetRepo _budgetRepo;

    public BudgetService(IBudgetRepo budgetRepo)
    {
        _budgetRepo = budgetRepo;
    }

    public decimal Query(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return 0;
        }

        var budgets = _budgetRepo.GetAll();

        if (start.ToString("yyyyMM") == end.ToString("yyyyMM"))
        {
            var queryDays = end.Day - start.Day + 1;
            return GetDailyBudget(start, budgets.FirstOrDefault(x => x.YearMonth == $"{start.Year}{start.Month.ToString("00")}")) * queryDays;
        }

        var current = start;

        var totalBudget = 0;

        while (current < new DateTime(end.Year, end.Month, 1).AddMonths(1))
        {
            if (current.ToString("yyyyMM") == start.ToString("yyyyMM"))
            {
                var queryDays = DateTime.DaysInMonth(start.Year, start.Month) - start.Day + 1;

                var budget = budgets.FirstOrDefault(x => x.YearMonth == $"{current.Year}{current.Month.ToString("00")}");
                totalBudget += GetDailyBudget(start, budget) * queryDays;
            }
            else if (current.ToString("yyyyMM") == end.ToString("yyyyMM"))
            {
                var budget = budgets.FirstOrDefault(x => x.YearMonth == $"{current.Year}{current.Month.ToString("00")}");
                totalBudget += GetDailyBudget(end, budget) * end.Day;
            }
            else
            {
                var budget = budgets.FirstOrDefault(x => x.YearMonth == $"{current.Year}{current.Month.ToString("00")}");
                totalBudget += GetDailyBudget(current, budget) * DateTime.DaysInMonth(current.Year, current.Month);
            }

            current = current.AddMonths(1);
        }

        return totalBudget;
    }

    private static int GetDailyBudget(DateTime budgetTime, Budget? budget)
    {
        if (budget == null)
        {
            return 0;
        }

        var dailyBudget = budget.Amount / DateTime.DaysInMonth(budgetTime.Year, budgetTime.Month);
        return dailyBudget;
    }
}

public interface IBudgetRepo
{
    List<Budget> GetAll();
}

public class Budget
{
    public int Amount { get; set; }
    public string YearMonth { get; set; }
}