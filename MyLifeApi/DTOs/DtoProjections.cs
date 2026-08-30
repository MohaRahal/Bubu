using System.Linq.Expressions;
using MyLifeApi.DTOs.Accounts;
using MyLifeApi.DTOs.Activities;
using MyLifeApi.DTOs.ActivityGoals;
using MyLifeApi.DTOs.Categories;
using MyLifeApi.DTOs.FinancialCategories;
using MyLifeApi.DTOs.Goals;
using MyLifeApi.DTOs.RecurringTransactions;
using MyLifeApi.DTOs.Transactions;
using MyLifeApi.DTOs.Transfers;
using MyLifeApi.DTOs.Users;
using MyLifeApi.Models;

namespace MyLifeApi.DTOs;

public static class DtoProjections
{
    public static readonly Expression<Func<User, UserResponse>> User = x => new(x.Id, x.Name, x.Email, x.AvatarUrl, x.BirthDate, x.Status, x.LastLoginAt, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<Category, CategoryResponse>> Category = x => new(x.Id, x.UserId, x.Name, x.Description, x.Icon, x.Color, x.IsActive, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<Activity, ActivityResponse>> Activity = x => new(x.Id, x.UserId, x.CategoryId, x.Title, x.Description, x.ActivityDate, x.StartedAt, x.EndedAt, x.DurationSeconds, x.Status, x.Source, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<Goal, GoalResponse>> Goal = x => new(x.Id, x.UserId, x.Title, x.Description, x.GoalType, x.TargetValue, x.Unit, x.StartDate, x.TargetDate, x.Status, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<ActivityGoal, ActivityGoalResponse>> ActivityGoal = x => new(x.ActivityId, x.GoalId, x.CreatedAt);
    public static readonly Expression<Func<Account, AccountResponse>> Account = x => new(x.Id, x.UserId, x.Name, x.AccountType, x.InitialBalance, x.IsActive, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<FinancialCategory, FinancialCategoryResponse>> FinancialCategory = x => new(x.Id, x.UserId, x.Name, x.CategoryType, x.Icon, x.Color, x.IsActive, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<RecurringTransaction, RecurringTransactionResponse>> RecurringTransaction = x => new(x.Id, x.UserId, x.AccountId, x.CategoryId, x.TransactionType, x.Description, x.Amount, x.Frequency, x.StartDate, x.EndDate, x.NextDueDate, x.IsActive, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<Transaction, TransactionResponse>> Transaction = x => new(x.Id, x.UserId, x.AccountId, x.CategoryId, x.RecurringTransactionId, x.TransactionType, x.Description, x.Amount, x.TransactionDate, x.Status, x.Notes, x.CreatedAt, x.UpdatedAt);
    public static readonly Expression<Func<Transfer, TransferResponse>> Transfer = x => new(x.Id, x.UserId, x.FromAccountId, x.ToAccountId, x.Amount, x.TransferDate, x.Description, x.Status, x.CreatedAt, x.UpdatedAt);
}
