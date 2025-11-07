using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности Transfer
/// </summary>
public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(t => t.Date)
            .IsRequired();
        
        builder.Property(t => t.FromAccountId)
            .IsRequired();
        
        builder.Property(t => t.ToAccountId)
            .IsRequired();
        
        builder.Property(t => t.FromAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        
        builder.Property(t => t.ToAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        
        builder.Property(t => t.Description)
            .HasMaxLength(1000);
        
        // Foreign Keys
        builder.HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(t => t.FromAccountId);
        builder.HasIndex(t => t.ToAccountId);
        builder.HasIndex(t => t.Date);
    }
}