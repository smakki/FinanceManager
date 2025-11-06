using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности TransactionsCategory
/// </summary>
public class TransactionsCategoryConfiguration : IEntityTypeConfiguration<TransactionsCategory>
{
    public void Configure(EntityTypeBuilder<TransactionsCategory> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();
        
        builder.Property(c => c.HolderId)
            .IsRequired();
        
        builder.Property(c => c.Income)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(c => c.Expense)
            .IsRequired()
            .HasDefaultValue(false);
        
        // SoftDeletableEntity properties
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Key
        builder.HasOne(c => c.Holder)
            .WithMany()
            .HasForeignKey(c => c.HolderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(c => c.HolderId);
        builder.HasIndex(c => c.IsDeleted);
    }
}