using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinanceManager.CatalogService.Abstractions.Services;
using FinanceManager.CatalogService.API.Extensions;
using FinanceManager.CatalogService.Contracts.DTOs.AccountTypes;
using FinanceManager.CatalogService.Contracts.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace FinanceManager.CatalogService.API.Controllers;

/// <summary>
/// Контроллер для управления типами счетов.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Контроллер для работы с типами счетов")]
[Produces("application/json")]
public class AccountTypeController(IAccountTypeService accountTypeService, ILogger logger) : ControllerBase
{
    
    /// <summary>
    /// Получает тип счета по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа счета (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 200 OK с данными типа счета (<see cref="CategoryDto"/>),
    /// - 400 Bad Request при невалидном идентификаторе,
    /// - 404 Not Found если тип счета не существует,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="200">Тип счета найден и возвращен.</response>
    /// <response code="400">Некорректный формат идентификатора.</response>
    /// <response code="404">Тип счета с указанным ID не найден.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// GET /api/v1/category/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение типа счета по идентификатору",
        Description = "Возвращает тип счета по указанному идентификатору")]
    [SwaggerResponse(200, "Тип счета успешно найден", typeof(AccountTypeDto))]
    [SwaggerResponse(400, "Некорректный идентификатор")]
    [SwaggerResponse(404, "Тип счета не найден")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<AccountTypeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос информации о категории по Id: {CategoryId}", id);
        
        var result = await accountTypeService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult(this);
    }
    
    /// <summary>
    /// Получение списка типов счетов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="filter">Параметры фильтрации и пагинации.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>ActionResult со списком типов счетов или соответствующим статусом ошибки.</returns>
    /// <example>
    /// Пример запроса:
    /// <code>
    /// GET /api/accounttype?ItemsPerPage=10&amp;Page=1&amp;RegistryHolderId=%7BGuid%7D&amp;NameContains=Food
    /// </code>
    /// </example>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение списка типов счетов с фильтрацией и пагинацией",
        Description = "Возвращает список типов счетов с возможностью фильтрации по различным параметрам")]
    [SwaggerResponse(200, "Список типов счетов успешно получен", typeof(ICollection<AccountTypeDto>))]
    [SwaggerResponse(400, "Некорректные параметры фильтрации")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ICollection<AccountTypeDto>>> Get(
        [FromQuery] AccountTypeFilterDto filter,
        CancellationToken cancellationToken)
    {
        logger.Information("Запрос списка типов счетов с фильтрацией: {@Filter}", filter);

        var result = await accountTypeService.GetPagedAsync(filter, cancellationToken);

        return result.ToActionResult(this);
    }

}