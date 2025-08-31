using FinanceManager.CatalogService.Abstractions.Services;
using FinanceManager.CatalogService.API.Extensions;
using FinanceManager.CatalogService.Contracts.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ILogger = Serilog.ILogger;

namespace FinanceManager.CatalogService.API.Controllers;

/// <summary>
/// Контроллер для управления категориями.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Контроллер для работы с категориями")]
[Produces("application/json")]
public class CategoryController(ICategoryService categoryService, ILogger logger) : ControllerBase
{
    /// <summary>
    /// Получает категорию по уникальному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 200 OK с данными категории (<see cref="CategoryDto"/>),
    /// - 400 Bad Request при невалидном идентификаторе,
    /// - 404 Not Found если категория не существует,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="200">Категория найдена и возвращена.</response>
    /// <response code="400">Некорректный формат идентификатора.</response>
    /// <response code="404">Категория с указанным ID не найдена.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// GET /api/v1/category/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение категории по идентификатору",
        Description = "Возвращает категорию по указанному идентификатору")]
    [SwaggerResponse(200, "Категория успешно найдена", typeof(CategoryDto))]
    [SwaggerResponse(400, "Некорректный идентификатор")]
    [SwaggerResponse(404, "Категория не найдена")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.Information("Запрос информации о категории по Id: {CategoryId}", id);
        
        var result = await categoryService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult(this);
    }
    
    /// <summary>
    /// Получение списка категорий с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="filter">Параметры фильтрации и пагинации.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>ActionResult со списком категорий или соответствующим статусом ошибки.</returns>
    /// <example>
    /// Пример запроса:
    /// <code>
    /// GET /api/category?ItemsPerPage=10&amp;Page=1&amp;RegistryHolderId=%7BGuid%7D&amp;NameContains=Food
    /// </code>
    /// </example>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение списка категорий с фильтрацией и пагинацией",
        Description = "Возвращает список категорий с возможностью фильтрации по различным параметрам")]
    [SwaggerResponse(200, "Список категорий успешно получен", typeof(ICollection<CategoryDto>))]
    [SwaggerResponse(400, "Некорректные параметры фильтрации")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ICollection<CategoryDto>>> Get(
        [FromQuery] CategoryFilterDto filter,
        CancellationToken cancellationToken)
    {
        logger.Information("Запрос списка категорий с фильтрацией: {@Filter}", filter);

        var result = await categoryService.GetPagedAsync(filter, cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получает все категории по идентификатору владельца реестра.
    /// </summary>
    /// <param name="registryHolderId">Идентификатор владельца реестра (GUID).</param>
    /// <param name="includeRelated">Включать ли связанные сущности (по умолчанию true).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 200 OK со списком категорий (<see cref="ICollection{CategoryDto}"/>),
    /// - 400 Bad Request при невалидном идентификаторе,
    /// - 404 Not Found если владелец реестра не найден,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="200">Список категорий успешно получен.</response>
    /// <response code="400">Некорректный формат идентификатора.</response>
    /// <response code="404">Владелец реестра с указанным ID не найден.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// GET /api/v1/category/registry-holder/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpGet("registry-holder/{registryHolderId:guid}")]
    [SwaggerOperation(
        Summary = "Получение категорий по идентификатору владельца реестра",
        Description = "Возвращает все категории, принадлежащие указанному владельцу реестра")]
    [SwaggerResponse(200, "Список категорий успешно получен", typeof(ICollection<CategoryDto>))]
    [SwaggerResponse(400, "Некорректный идентификатор")]
    [SwaggerResponse(404, "Владелец реестра не найден")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<ICollection<CategoryDto>>> GetByRegistryHolderId(
        Guid registryHolderId,
        [FromQuery] bool includeRelated = true,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос категорий по RegistryHolderId: {RegistryHolderId}, IncludeRelated: {IncludeRelated}", 
            registryHolderId, includeRelated);
        
        var result = await categoryService.GetByRegistryHolderIdAsync(registryHolderId, includeRelated, cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>
    /// Создает новую категорию.
    /// </summary>
    /// <param name="createDto">Данные для создания категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 201 Created с данными созданной категории (<see cref="CategoryDto"/>),
    /// - 400 Bad Request при невалидных данных,
    /// - 404 Not Found если родительская категория не существует,
    /// - 409 Conflict при конфликте данных,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="201">Категория успешно создана.</response>
    /// <response code="400">Некорректные данные для создания.</response>
    /// <response code="404">Родительская категория не найдена.</response>
    /// <response code="409">Конфликт данных.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// POST /api/v1/category
    /// {
    ///   "registryHolderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "Продукты",
    ///   "income": false,
    ///   "expense": true,
    ///   "emoji": "🛒"
    /// }
    /// </example>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Создание новой категории",
        Description = "Создает новую категорию с указанными параметрами")]
    [SwaggerResponse(201, "Категория успешно создана", typeof(CategoryDto))]
    [SwaggerResponse(400, "Некорректные данные")]
    [SwaggerResponse(404, "Родительская категория не найдена")]
    [SwaggerResponse(409, "Конфликт данных")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryDto createDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос на создание категории: {@CreateDto}", createDto);
        
        var result = await categoryService.CreateAsync(createDto, cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>
    /// Обновляет существующую категорию.
    /// </summary>
    /// <param name="updateDto">Данные для обновления категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 200 OK с данными обновленной категории (<see cref="CategoryDto"/>),
    /// - 400 Bad Request при невалидных данных,
    /// - 404 Not Found если категория не существует,
    /// - 409 Conflict при конфликте данных,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="200">Категория успешно обновлена.</response>
    /// <response code="400">Некорректные данные для обновления.</response>
    /// <response code="404">Категория не найдена.</response>
    /// <response code="409">Конфликт данных.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// PUT /api/v1/category
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "name": "Продукты питания",
    ///   "emoji": "🍎"
    /// }
    /// </example>
    [HttpPut]
    [SwaggerOperation(
        Summary = "Обновление существующей категории",
        Description = "Обновляет существующую категорию с указанными параметрами")]
    [SwaggerResponse(200, "Категория успешно обновлена", typeof(CategoryDto))]
    [SwaggerResponse(400, "Некорректные данные")]
    [SwaggerResponse(404, "Категория не найдена")]
    [SwaggerResponse(409, "Конфликт данных")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult<CategoryDto>> Update(
        [FromBody] UpdateCategoryDto updateDto,
        CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос на обновление категории: {@UpdateDto}", updateDto);
        
        var result = await categoryService.UpdateAsync(updateDto, cancellationToken);

        return result.ToActionResult(this);
    }

    /// <summary>
    /// Удаляет категорию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат выполнения операции:
    /// - 204 No Content при успешном удалении,
    /// - 400 Bad Request при невалидном идентификаторе,
    /// - 404 Not Found если категория не существует,
    /// - 409 Conflict если категория не может быть удалена,
    /// - 500 Internal Server Error при внутренних ошибках.
    /// </returns>
    /// <response code="204">Категория успешно удалена.</response>
    /// <response code="400">Некорректный формат идентификатора.</response>
    /// <response code="404">Категория с указанным ID не найдена.</response>
    /// <response code="409">Категория не может быть удалена.</response>
    /// <response code="500">Ошибка на сервере.</response>
    /// <example>
    /// Пример запроса:
    /// DELETE /api/v1/category/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </example>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Удаление категории по идентификатору",
        Description = "Удаляет категорию по указанному идентификатору")]
    [SwaggerResponse(204, "Категория успешно удалена")]
    [SwaggerResponse(400, "Некорректный идентификатор")]
    [SwaggerResponse(404, "Категория не найдена")]
    [SwaggerResponse(409, "Категория не может быть удалена")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        logger.Information("Запрос на удаление категории по Id: {CategoryId}", id);
        
        var result = await categoryService.DeleteAsync(id, cancellationToken);

        return result.ToActionResult(this);
    }
}