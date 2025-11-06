using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности TransactionsAccountType
/// </summary>
public class TransactionsAccountTypeConfiguration : IEntityTypeConfiguration<TransactionsAccountType>
{
    public void Configure(EntityTypeBuilder<TransactionsAccountType> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .ValueGeneratedNever();
        
        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasIndex(t => t.Code)
            .IsUnique();
    }
}