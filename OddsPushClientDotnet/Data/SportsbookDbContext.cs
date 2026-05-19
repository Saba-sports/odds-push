using Microsoft.EntityFrameworkCore;

namespace OddsPushClient.Data;

public class SportsbookDbContext : DbContext
{
    public SportsbookDbContext(DbContextOptions<SportsbookDbContext> options) : base(options)
    {
    }

    public DbSet<SportEvent> Events { get; set; }
    public DbSet<Market> Markets { get; set; }
    public DbSet<Selection> Selections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SportEvent>()
            .HasMany(e => e.Markets)
            .WithOne(m => m.Event)
            .HasForeignKey(m => m.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Market>()
            .HasMany(m => m.Selections)
            .WithOne(o => o.Market)
            .HasForeignKey(o => o.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Selection>()
            .HasIndex(o => new { o.MarketId, o.SelectionKey })
            .IsUnique();
    }
}
