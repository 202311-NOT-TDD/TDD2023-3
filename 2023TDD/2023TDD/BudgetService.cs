#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace _2023TDD;

public class Period
{
    public Period(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime End { get; private set; }
    public DateTime Start { get; private set; }
}

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
                var overlappingDays = OverlappingDays(new Period(start, end), budget);

                totalBudget += budget.GetDailyBudget() * overlappingDays;
            }

            current = current.AddMonths(1);
        }

        return totalBudget;
    }

    private static int OverlappingDays(Period period, Budget budget)
    {
        DateTime overlappingEnd;
        DateTime overlappingStart;
        if (budget.YearMonth == period.Start.ToString("yyyyMM"))
        {
            overlappingEnd = budget.LastDay();
            overlappingStart = period.Start;
        }
        else if (budget.YearMonth == period.End.ToString("yyyyMM"))
        {
            overlappingEnd = period.End;
            overlappingStart = budget.FirstDay();
        }
        else
        {
            overlappingEnd = budget.LastDay();
            overlappingStart = budget.FirstDay();
        }

        return (overlappingEnd - overlappingStart).Days + 1;
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