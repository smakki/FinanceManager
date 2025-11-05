using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionAccounts;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с банковскими счетами транзакций.
/// Предоставляет методы фильтрации, проверки счета по умолчанию.
/// </summary>
public class TransactionAccountRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<TransactionsAccount, TransactionAccountFilterDto>(context, logger), 
      ITransactionAccountRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Включает связанные сущности для счета.
    /// </summary>
    protected override IQueryable<TransactionsAccount> IncludeRelatedEntities(
        IQueryable<TransactionsAccount> query)
    {
        return query
            .Include(a => a.AccountType)
            .Include(a => a.Currency)
            .Include(a => a.Holder);
    }

    /// <summary>
    /// Применяет фильтры к запросу счетов.
    /// </summary>
    protected override IQueryable<TransactionsAccount> SetFilters(
        TransactionAccountFilterDto filter,
        IQueryable<TransactionsAccount> query)
    {
        if (filter.TransactionHolderId.HasValue)
            query = query.Where(a => a.HolderId == filter.TransactionHolderId);
        
        if (filter.AccountTypeId.HasValue)
            query = query.Where(a => a.AccountTypeId == filter.AccountTypeId);
        
        if (filter.CurrencyId.HasValue)
            query = query.Where(a => a.CurrencyId == filter.CurrencyId);
        
        if (filter.CreditLimitFrom.HasValue)
            query = query.Where(a => a.CreditLimit >= filter.CreditLimitFrom);
        
        if (filter.CreditLimitTo.HasValue)
            query = query.Where(a => a.CreditLimit <= filter.CreditLimitTo);


        return query;
    }
}
