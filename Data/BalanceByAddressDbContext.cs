using Argus_BAVer2.Data.Models;
using Cardano.Sync.Data;
using Microsoft.EntityFrameworkCore;

public class BalanceByAddressDbContext
(
    DbContextOptions<BalanceByAddressDbContext> options,
    IConfiguration configuration 
) : CardanoDbContext(options, configuration)
{
    public DbSet<BalanceByAddress> BalanceByAddress {get; set; }
    public DbSet<InputsBySlot> InputsBySlot { get; set; }
    public DbSet<OutputsBySlot> OutputsBySlot { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BalanceByAddress>(entity => {
            entity.HasKey(e => e.Address);
            entity.Property( e => e.Balance)
                .HasColumnName("Balance");
        });

        modelBuilder.Entity<InputsBySlot>(entity => {
            entity.HasKey(e => new {e.TxHash, e.OutputIndex});
            entity.Property( e => e.Slot)
                .HasColumnName("Slot");
        });

        modelBuilder.Entity<OutputsBySlot>(entity => {
            entity.HasKey(e => new {e.TxHash, e.TxIndex});
            entity.Property( e => e.Value)
                .HasColumnName("Value");
            entity.Property(e => e.Slot)
                .HasColumnName("Slot");
        });
    }
}