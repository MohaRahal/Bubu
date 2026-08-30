using Microsoft.EntityFrameworkCore;
using MyLifeApi.Models;

namespace MyLifeApi.Data;

public class MyLifeDbContext(DbContextOptions<MyLifeDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<ActivityGoal> ActivityGoals => Set<ActivityGoal>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FinancialCategory> FinancialCategories => Set<FinancialCategory>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyLifeDbContext).Assembly);
    }
}
