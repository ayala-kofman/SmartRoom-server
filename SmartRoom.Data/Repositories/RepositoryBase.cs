using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartRoom.Core.Exceptions;
using SmartRoom.Data.Configurations;

namespace SmartRoom.Data.Repositories;

/// <summary>Shared SaveChanges for all repositories: turns EF/Postgres race errors into ConcurrencyConflictException.</summary>
public abstract class RepositoryBase
{
    protected readonly AppDbContext Db;

    protected RepositoryBase(AppDbContext db)
    {
        Db = db;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // The Version in the WHERE clause no longer matched: someone else saved first.
            throw new ConcurrencyConflictException("The data was changed by another user.", ex);
        }
        catch (DbUpdateException ex) when (IsRaceUniqueViolation(ex))
        {
            throw new ConcurrencyConflictException("Another request created the same record first.", ex);
        }
    }

    private static bool IsRaceUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: BookingConfiguration.ActiveBookingIndexName
                or RoomSlotConfiguration.UniqueSlotIndexName
                or UserConfiguration.UniqueIdentityNumberIndexName
        };
}
