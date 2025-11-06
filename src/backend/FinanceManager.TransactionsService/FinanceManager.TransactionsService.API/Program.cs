using System.Threading.Tasks;
using FinanceManager.TransactionsService.API.Extensions;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinanceManager.TransactionsService.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDatabase(builder.Configuration, builder.Environment.IsDevelopment());
        builder.Host.AddLogging(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddTransactionDataLoaderJob();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        await app.UseMigrationAsync();
        app.MapControllers();

        app.Run();
    }
}