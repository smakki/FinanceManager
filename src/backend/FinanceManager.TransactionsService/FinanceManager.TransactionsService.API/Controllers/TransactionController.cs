using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Contracts.DTOs.Transactions;
using FinanceManager.TransactionsService.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace FinanceManager.TransactionsService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionController(ILogger logger, ITransactionService transactionService)
    : ControllerBase
{
    [HttpGet(Name = "GetTransactions")]
    public async Task<ActionResult<ICollection<TransactionDto>>> Get(
        [FromQuery] TransactionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос списка владельцев справочников с фильтрацией: {@Filter}", filter);
        var result = await transactionService.GetPagedAsync(filter, cancellationToken);
        return result.ToActionResult(this);
    }
}