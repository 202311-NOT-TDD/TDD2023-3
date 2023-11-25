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
            return budgets.FirstOrDefault(x => x.YearMonth == $"{start.Year}{start.Month.ToString("00")}").GetDailyBudget() * queryDays;
        }

        var current = start;

        var totalBudget = 0m;

        while (current < new DateTime(end.Year, end.Month, 1).AddMonths(1))
        {
            var budget = budgets.FirstOrDefault(x => x.YearMonth == current.ToString("yyyyMM"));
            if (budget != null)
            {
                int overlappingDays;
                DateTime overlappingEnd;
                DateTime overlappingStart;
                if (current.ToString("yyyyMM") == start.ToString("yyyyMM"))
                {
                    overlappingEnd = budget.LastDay();
                    overlappingStart = start;
                    // overlappingDays = (overlappingEnd - overlappingStart).Days + 1;
                }
                else if (current.ToString("yyyyMM") == end.ToString("yyyyMM"))
                {
                    overlappingEnd = end;
                    overlappingStart = budget.FirstDay();
                    // overlappingDays = (overlappingEnd - overlappingStart).Days + 1;
                }
                else
                {
                    overlappingEnd = budget.LastDay();
                    overlappingStart = budget.FirstDay();
                    // overlappingDays = (overlappingEnd - overlappingStart).Days + 1;
                }

                overlappingDays = (overlappingEnd - overlappingStart).Days + 1;

                totalBudget += budget.GetDailyBudget() * overlappingDays;
            }

            current = current.AddMonths(1);
        }

        return totalBudget;
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

    public DateTime FirstDay()
    {
        return DateTime.ParseExact(YearMonth, "yyyyMM", null);
    }

    public decimal GetDailyBudget()
    {
        return Amount / (decimal)Days();
    }

    public DateTime LastDay()
    {
        return DateTime.ParseExact(YearMonth + Days(), "yyyyMMdd", null);
    }

    private int Days()
    {
        return DateTime.DaysInMonth(FirstDay().Year, FirstDay().Month);
    }
}