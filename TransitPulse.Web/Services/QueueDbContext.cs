using Microsoft.EntityFrameworkCore;
using TransitPulse.Web.Models;

namespace TransitPulse.Web.Services;

public class QueueDbContext : DbContext
{
    public const string SqliteDbFilename = "app.db";

    public DbSet<QueueState> Queues { get; set; } = default!;

    public QueueDbContext(DbContextOptions<QueueDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QueueState>().HasKey(nameof(QueueState.Name));
        modelBuilder.Entity<QueueState>().Property(x => x.Name).UseCollation("nocase");
    }
}