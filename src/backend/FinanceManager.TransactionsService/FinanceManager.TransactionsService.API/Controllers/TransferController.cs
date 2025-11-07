using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Contracts.DTOs.Transfers;
using FinanceManager.TransactionsService.API.Extensions;
using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace FinanceManager.TransactionsService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TransferController(ILogger logger, ITransferService transferService) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "Get transfer by Id")]
    public async Task<ActionResult<TransferDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос информации о переводе по Id: {TransferId}", id);
        var result = await transferService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet(Name = "Get list transfers")]
    public async Task<ActionResult<ICollection<Transfer>>> Get(
        [FromQuery] TransferFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос списка переводов с фильтрацией: {@Filter}", filter);
        var result = await transferService.GetPagedAsync(filter, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost(Name = "Create transfer")]
    public async Task<ActionResult<Transfer>> Create(
        [FromQuery] CreateTransferDto createDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос создания перевода: {@CreateDto}", createDto);
        var result = await transferService.CreateAsync(createDto, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut(Name = "Update transfer")]
    public async Task<ActionResult<TransferDto>> Update(
        [FromQuery] UpdateTransferDto updateDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос обновления перевода: {@UpdateDto}", updateDto);
        var result = await transferService.UpdateAsync(updateDto, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete(Name = "Delete transfer")]
    public async Task<ActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос удаления перевода с Id: {@ID}", id);
        var result = await transferService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
