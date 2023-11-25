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

        var queryTime = start;

        var totalBudget = 0;

        while (queryTime <= end || (queryTime.Month == end.Month))
        {
            if (start.Year == end.Year && start.Month == end.Month)
            {
                var queryDays = end.Day - start.Day + 1;
                totalBudget += GetDailyBudget(start, budgets) * queryDays;
            }
            else if (queryTime.Year == start.Year && queryTime.Month == start.Month)
            {
                var queryDays = DateTime.DaysInMonth(start.Year, start.Month) - start.Day + 1;

                totalBudget += GetDailyBudget(start, budgets) * queryDays;
            }
            else if (queryTime.Year == end.Year && queryTime.Month == end.Month)
            {
                totalBudget += GetDailyBudget(end, budgets) * end.Day;
            }
            else
            {
                totalBudget += GetDailyBudget(queryTime, budgets) * DateTime.DaysInMonth(queryTime.Year, queryTime.Month);
            }

            queryTime = queryTime.AddMonths(1);
        }

        return totalBudget;
    }

    private static int GetDailyBudget(DateTime budgetTime, List<Budget> budgets)
    {
        var budget = budgets.FirstOrDefault(x => x.YearMonth == $"{budgetTime.Year}{budgetTime.Month.ToString("00")}");
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