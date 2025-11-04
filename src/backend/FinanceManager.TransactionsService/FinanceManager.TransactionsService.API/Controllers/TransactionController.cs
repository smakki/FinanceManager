using System;
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
    
    [HttpGet("{id:guid}", Name = "Get transaction by Id")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос информации о категории по Id: {CategoryId}", id);
        
        var result = await transactionService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult(this);
    }
    
    [HttpGet(Name = "Get list transactions")]
    public async Task<ActionResult<ICollection<TransactionDto>>> Get(
        [FromQuery] TransactionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос списка владельцев справочников с фильтрацией: {@Filter}", filter);
        var result = await transactionService.GetPagedAsync(filter, cancellationToken);
        return result.ToActionResult(this);
    }
    
    [HttpPost(Name = "Create transactions")]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromQuery] CreateTransactionDto createDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос списка владельцев справочников с фильтрацией: {@CreateDto}", createDto);
        var result = await transactionService.CreateAsync(createDto, cancellationToken);
        return result.ToActionResult(this);
    }
    
    [HttpPut(Name = "Update transactions")]
    public async Task<ActionResult<TransactionDto>> Update(
        [FromQuery] UpdateTransactionDto updateDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос обновления транзакции: {@UpdateDto}", updateDto);
        var result = await transactionService.UpdateAsync(updateDto, cancellationToken);
        return result.ToActionResult(this);
    }
    
    [HttpDelete(Name = "Delete transactions")]
    public async Task<ActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос удаления транзакции с id : {@ID}", id);
        var result = await transactionService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
    
}