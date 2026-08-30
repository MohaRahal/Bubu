using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Accounts;

public sealed record AccountRequest(
    ulong UserId,
    [Required, StringLength(100)] string Name,
    AccountType AccountType,
    [Range(typeof(decimal), "-999999999999.99", "999999999999.99")] decimal InitialBalance = 0,
    bool IsActive = true);

public sealed record AccountResponse(
    ulong Id, ulong UserId, string Name, AccountType AccountType,
    decimal InitialBalance, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);
