using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.TransactionsService.EntityFramework;

public class DatabaseContext(DbContextOptions<DatabaseContext> options):DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Сохраняет все изменения в базе данных в рамках текущей транзакции.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество затронутых записей.</returns>
    public async Task<int> CommitAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}