using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности TransactionHolder
/// </summary>
public class TransactionHolderConfiguration : IEntityTypeConfiguration<TransactionHolder>
{
    public void Configure(EntityTypeBuilder<TransactionHolder> builder)
    {
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Id)
            .ValueGeneratedNever();
        
        builder.Property(h => h.Role)
            .IsRequired();
        
        builder.Property(h => h.TelegramId)
            .IsRequired(false);
        
        builder.HasIndex(h => h.TelegramId)
            .IsUnique();
    }
}