using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности TransactionsCurrency
/// </summary>
public class TransactionsCurrencyConfiguration : IEntityTypeConfiguration<TransactionsCurrency>
{
    public void Configure(EntityTypeBuilder<TransactionsCurrency> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();
        
        builder.Property(c => c.CharCode)
            .IsRequired()
            .HasMaxLength(3);
        
        builder.Property(c => c.NumCode)
            .IsRequired()
            .HasMaxLength(3);
        
        builder.HasIndex(c => c.CharCode)
            .IsUnique();
        
        builder.HasIndex(c => c.NumCode)
            .IsUnique();
    }
}