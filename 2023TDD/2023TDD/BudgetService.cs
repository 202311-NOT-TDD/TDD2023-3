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
            var budget = budgets.FirstOrDefault(x => x.YearMonth == current.ToString("yyyyMM"));
            if (budget != null)
            {
                int overlappingDays;
                if (current.ToString("yyyyMM") == start.ToString("yyyyMM"))
                {
                    overlappingDays = DateTime.DaysInMonth(start.Year, start.Month) - start.Day + 1;
                }
                else if (current.ToString("yyyyMM") == end.ToString("yyyyMM"))
                {
                    overlappingDays = end.Day;
                }
                else
                {
                    overlappingDays = DateTime.DaysInMonth(current.Year, current.Month);
                }

                totalBudget += GetDailyBudget(current, budget) * overlappingDays;
            }

            current = current.AddMonths(1);
        }

        return totalBudget;
    }

    private static int GetDailyBudget(DateTime budgetTime, Budget budget)
    {
        var firstDayOfBudget = DateTime.ParseExact(budget.YearMonth, "yyyyMM", null);
        var daysInMonth = DateTime.DaysInMonth(firstDayOfBudget.Year, firstDayOfBudget.Month);
        return budget.Amount / daysInMonth;
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