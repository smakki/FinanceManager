using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности Transaction
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(t => t.Date)
            .IsRequired();
        
        builder.Property(t => t.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        
        builder.Property(t => t.Description)
            .HasMaxLength(1000);
        
        // Foreign Keys
        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.Date);
    }
}