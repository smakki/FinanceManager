using DotNetEnv;
using FinanceManager.CatalogService.API.Extensions;
using FinanceManager.CatalogService.API.Middleware;
using FinanceManager.CatalogService.EntityFramework;
using FinanceManager.CatalogService.EntityFramework.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
// Add services to the container.
builder.Host.AddLogging(builder.Configuration);
builder.Services
    .AddExceptionHandling()
    .AddDatabase(builder.Configuration, builder.Environment.IsDevelopment())
    .AddSeeding()
    .AddApplication();

builder.Services.AddWebApiConfiguration();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwagger(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestEnrichmentMiddleware>();
app.UseExceptionHandler();

await app.UseMigrationAsync();
await app.SeedDatabaseAsync();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();