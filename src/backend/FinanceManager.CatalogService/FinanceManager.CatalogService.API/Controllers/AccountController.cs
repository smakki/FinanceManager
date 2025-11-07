using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceManager.CatalogService.Abstractions.Services;
using FinanceManager.CatalogService.Contracts.DTOs.Accounts;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using FluentResults;
using FinanceManager.CatalogService.API.Extensions;
using FinanceManager.CatalogService.Domain.Entities;

namespace FinanceManager.CatalogService.API.Controllers;

/// <summary>
/// Контроллер для управления банковскими счетами.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Контроллер для работы с банковскими счетами")]
[Produces("application/json")]
public class AccountController(IAccountService accountService, ILogger logger) : ControllerBase
{
    /// <summary>
    /// Получает счет по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор счета (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 200 OK с данными счета (<see cref="AccountDto"/>),
    /// - 404 Not Found если счет не найден,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение счета по идентификатору",
        Description = "Возвращает счет по указанному идентификатору")]
    [SwaggerResponse(200, "Счет успешно найден", typeof(AccountDto))]
    [SwaggerResponse(404, "Счет не найден")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос информации о счете по Id: {AccountId}", id);
        var result = await accountService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получение списка счетов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="filter">Параметры фильтрации и пагинации.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список счетов или ошибка.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение списка счетов с фильтрацией и пагинацией",
        Description = "Возвращает список счетов с возможностью фильтрации")]
    [SwaggerResponse(200, "Список счетов успешно получен", typeof(ICollection<AccountDto>))]
    [SwaggerResponse(400, "Некорректные параметры фильтрации")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ICollection<Account>>> Get(
        [FromQuery] AccountFilterDto filter,
        CancellationToken cancellationToken)
    {
        logger.Information("Запрос списка счетов с фильтрацией: {@Filter}", filter);
        var result = await accountService.GetPagedAsync(filter, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получение счета по умолчанию для пользователя.
    /// </summary>
    [HttpGet("default/{registryHolderId:guid}")]
    [SwaggerOperation(Summary = "Получение счета по умолчанию", Description = "Возвращает счет по умолчанию для указанного владельца")]
    [SwaggerResponse(200, "Счет по умолчанию найден", typeof(AccountDto))]
    [SwaggerResponse(404, "Счет по умолчанию не найден")]
    public async Task<ActionResult<AccountDto>> GetDefaultAccount(Guid registryHolderId, CancellationToken cancellationToken)
    {
        logger.Information("Запрос счета по умолчанию для владельца: {RegistryHolderId}", registryHolderId);
        var result = await accountService.GetDefaultAccountAsync(registryHolderId, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Создает новый счет.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Создание нового счета", Description = "Создает новый банковский счет")]
    [SwaggerResponse(201, "Счет успешно создан", typeof(AccountDto))]
    [SwaggerResponse(400, "Некорректные данные для создания счета")]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountDto createDto, CancellationToken cancellationToken)
    {
        logger.Information("Создание нового счета: {@CreateDto}", createDto);
        var result = await accountService.CreateAsync(createDto, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Обновляет существующий счет.
    /// </summary>
    [HttpPut]
    [SwaggerOperation(Summary = "Обновление счета", Description = "Обновляет существующий счет")]
    [SwaggerResponse(200, "Счет успешно обновлен", typeof(AccountDto))]
    [SwaggerResponse(400, "Некорректные данные для обновления")]
    [SwaggerResponse(404, "Счет не найден")]
    public async Task<ActionResult<AccountDto>> Update([FromBody] UpdateAccountDto updateDto, CancellationToken cancellationToken)
    {
        logger.Information("Обновление счета: {@UpdateDto}", updateDto);
        var result = await accountService.UpdateAsync(updateDto, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Мягкое удаление счета.
    /// </summary>
    [HttpDelete("{id:guid}/soft")]
    [SwaggerOperation(Summary = "Мягкое удаление счета", Description = "Помечает счет как удаленный без фактического удаления")]
    [SwaggerResponse(200, "Счет успешно удален")]
    [SwaggerResponse(404, "Счет не найден")]
    public async Task<ActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Мягкое удаление счета: {AccountId}", id);
        var result = await accountService.SoftDeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Восстанавливает ранее удалённый счет.
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [SwaggerOperation(Summary = "Восстановление счета", Description = "Восстанавливает ранее мягко удаленный счет")]
    [SwaggerResponse(200, "Счет успешно восстановлен")]
    [SwaggerResponse(404, "Счет не найден")]
    public async Task<ActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Восстановление счета: {AccountId}", id);
        var result = await accountService.RestoreDeletedAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
