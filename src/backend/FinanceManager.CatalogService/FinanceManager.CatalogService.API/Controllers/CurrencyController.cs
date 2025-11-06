using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceManager.CatalogService.Abstractions.Services;
using FinanceManager.CatalogService.Contracts.DTOs.Currencies;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using FluentResults;
using FinanceManager.CatalogService.API.Extensions;

namespace FinanceManager.CatalogService.API.Controllers;

/// <summary>
/// Контроллер для управления справочником валют.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Контроллер для работы с валютами")]
[Produces("application/json")]
public class CurrencyController(ICurrencyService currencyService, ILogger logger) : ControllerBase
{
    /// <summary>
    /// Получение валюты по идентификатору.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Получение валюты по Id", Description = "Возвращает валюту по указанному идентификатору")]
    [SwaggerResponse(200, "Валюта успешно найдена", typeof(CurrencyDto))]
    [SwaggerResponse(404, "Валюта не найдена")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<CurrencyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос валюты по Id: {CurrencyId}", id);
        var result = await currencyService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получение списка валют с фильтрацией и пагинацией.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Получение списка валют", Description = "Возвращает список валют с фильтрацией и пагинацией")]
    [SwaggerResponse(200, "Список валют успешно получен", typeof(ICollection<CurrencyDto>))]
    [SwaggerResponse(400, "Некорректные параметры фильтрации")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ICollection<CurrencyDto>>> Get([FromQuery] CurrencyFilterDto filter, CancellationToken cancellationToken)
    {
        logger.Information("Запрос списка валют с фильтром: {@Filter}", filter);
        var result = await currencyService.GetPagedAsync(filter, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Создание новой валюты.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Создание валюты", Description = "Создает новую валюту")]
    [SwaggerResponse(201, "Валюта успешно создана", typeof(CurrencyDto))]
    [SwaggerResponse(400, "Некорректные данные для создания")]
    public async Task<ActionResult<CurrencyDto>> Create([FromBody] CreateCurrencyDto createDto, CancellationToken cancellationToken)
    {
        logger.Information("Создание новой валюты: {@CreateDto}", createDto);
        var result = await currencyService.CreateAsync(createDto, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Обновление существующей валюты.
    /// </summary>
    [HttpPut]
    [SwaggerOperation(Summary = "Обновление валюты", Description = "Обновляет данные существующей валюты")]
    [SwaggerResponse(200, "Валюта успешно обновлена", typeof(CurrencyDto))]
    [SwaggerResponse(400, "Некорректные данные для обновления")]
    [SwaggerResponse(404, "Валюта не найдена")]
    public async Task<ActionResult<CurrencyDto>> Update([FromBody] UpdateCurrencyDto updateDto, CancellationToken cancellationToken)
    {
        logger.Information("Обновление валюты: {@UpdateDto}", updateDto);
        var result = await currencyService.UpdateAsync(updateDto, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Мягкое удаление валюты.
    /// </summary>
    [HttpDelete("{id:guid}/soft")]
    [SwaggerOperation(Summary = "Мягкое удаление валюты", Description = "Помечает валюту как удалённую")]
    [SwaggerResponse(200, "Валюта успешно удалена")]
    [SwaggerResponse(404, "Валюта не найдена")]
    public async Task<ActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Мягкое удаление валюты: {CurrencyId}", id);
        var result = await currencyService.SoftDeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Восстановление ранее удалённой валюты.
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [SwaggerOperation(Summary = "Восстановление валюты", Description = "Восстанавливает ранее мягко удалённую валюту")]
    [SwaggerResponse(200, "Валюта успешно восстановлена")]
    [SwaggerResponse(404, "Валюта не найдена")]
    public async Task<ActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Восстановление валюты: {CurrencyId}", id);
        var result = await currencyService.RestoreAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Жёсткое удаление валюты.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Жёсткое удаление валюты", Description = "Удаляет валюту из базы данных")]
    [SwaggerResponse(200, "Валюта успешно удалена")]
    [SwaggerResponse(404, "Валюта не найдена")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Жесткое удаление валюты: {CurrencyId}", id);
        var result = await currencyService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
