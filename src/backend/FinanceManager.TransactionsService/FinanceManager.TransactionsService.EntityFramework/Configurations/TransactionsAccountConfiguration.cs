using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.TransactionsService.EntityFramework.Configurations;

/// <summary>
/// Конфигурация для сущности TransactionsAccount
/// </summary>
public class TransactionsAccountConfiguration : IEntityTypeConfiguration<TransactionsAccount>
{
    public void Configure(EntityTypeBuilder<TransactionsAccount> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .ValueGeneratedNever();
        
        builder.Property(a => a.AccountTypeId)
            .IsRequired();
        
        builder.Property(a => a.CurrencyId)
            .IsRequired();
        
        builder.Property(a => a.HolderId)
            .IsRequired();
        
        builder.Property(a => a.CreditLimit)
            .HasColumnType("numeric(18,2)")
            .IsRequired(false);
        
        builder.Property(a => a.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
        
        // SoftDeletableEntity properties
        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Foreign Keys
        builder.HasOne(a => a.AccountType)
            .WithMany()
            .HasForeignKey(a => a.AccountTypeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.Currency)
            .WithMany()
            .HasForeignKey(a => a.CurrencyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.Holder)
            .WithMany()
            .HasForeignKey(a => a.HolderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(a => a.AccountTypeId);
        builder.HasIndex(a => a.CurrencyId);
        builder.HasIndex(a => a.HolderId);
        builder.HasIndex(a => a.IsArchived);
        builder.HasIndex(a => a.IsDeleted);
    }
}
