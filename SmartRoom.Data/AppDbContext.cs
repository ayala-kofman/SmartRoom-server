using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.Entities;

namespace SmartRoom.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClassGroup> ClassGroups => Set<ClassGroup>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<LessonPeriod> LessonPeriods => Set<LessonPeriod>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<BookingPeriod> BookingPeriods => Set<BookingPeriod>();
    public DbSet<RoomSlot> RoomSlots => Set<RoomSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration in the Configurations folder.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RenewSlotVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RenewSlotVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Gives every modified slot a new Version. EF still sends the ORIGINAL Version in the WHERE clause,
    /// so a second teacher saving the same slot updates 0 rows and gets DbUpdateConcurrencyException.
    /// </summary>
    private void RenewSlotVersions()
    {
        foreach (var entry in ChangeTracker.Entries<RoomSlot>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version = Guid.NewGuid();
            }
        }
    }
}
