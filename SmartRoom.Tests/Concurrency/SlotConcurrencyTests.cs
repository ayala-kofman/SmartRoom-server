using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Exceptions;
using SmartRoom.Data;
using SmartRoom.Data.Repositories;

namespace SmartRoom.Tests.Concurrency;

/// <summary>
/// Proves that the slot's Version token stops a save based on stale data.
/// SQLite in memory runs real SQL with a real WHERE clause (EF's InMemory provider would not check the token).
/// </summary>
public class SlotConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly int _slotId;

    public SlotConcurrencyTests()
    {
        // 1. One in-memory database that lives as long as this connection stays open; schema from AppDbContext.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var setup = new AppDbContext(_options);
        setup.Database.EnsureCreated();

        // 2. One room and one available slot.
        var room = new RegularRoom { RoomNumber = "T-1", Name = "Test room", Floor = 1, Quality = QualityLevel.Standard, SeatCount = 30 };
        var slot = new RoomSlot { Room = room, Date = new DateOnly(2026, 10, 22), LessonPeriodId = 1, Status = SlotStatus.Available };
        setup.RoomSlots.Add(slot);
        setup.SaveChanges();
        _slotId = slot.Id;
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task TwoContextsBookSameSlot_SecondSaveThrowsConcurrencyException()
    {
        // 3. Two separate contexts on the same database, like two teachers' requests.
        await using var first = new AppDbContext(_options);
        await using var second = new AppDbContext(_options);

        // 4. Both read the same slot and both see it available.
        var slotOfFirst = await first.RoomSlots.SingleAsync(s => s.Id == _slotId);
        var slotOfSecond = await second.RoomSlots.SingleAsync(s => s.Id == _slotId);
        Assert.Equal(SlotStatus.Available, slotOfFirst.Status);
        Assert.Equal(SlotStatus.Available, slotOfSecond.Status);

        // 5. Both mark it booked.
        slotOfFirst.Status = SlotStatus.Booked;
        slotOfSecond.Status = SlotStatus.Booked;

        // 6. The first save succeeds.
        await first.SaveChangesAsync();

        // 7. The second save is based on a Version that no longer exists: 0 rows updated → exception.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());

        // 8. A third context sees the slot booked once, with the Version written by the first save.
        await using var third = new AppDbContext(_options);
        var stored = await third.RoomSlots.AsNoTracking().SingleAsync(s => s.Id == _slotId);
        Assert.Equal(SlotStatus.Booked, stored.Status);
        Assert.Equal(slotOfFirst.Version, stored.Version);
        Assert.NotEqual(slotOfSecond.Version, stored.Version);
    }

    [Fact]
    public async Task TwoBookingsThroughRealRepositories_SecondSaveThrowsCoreConcurrencyConflictException()
    {
        // The same race through the real repositories: the EF exception must become the Core exception,
        // which is what BookingService catches and turns into 409.
        await using var first = new AppDbContext(_options);
        await using var second = new AppDbContext(_options);
        var slotsOfFirst = new RoomSlotRepository(first);
        var slotsOfSecond = new RoomSlotRepository(second);

        var slotOfFirst = (await slotsOfFirst.GetForUpdateAsync(_slotId, CancellationToken.None))!;
        var slotOfSecond = (await slotsOfSecond.GetForUpdateAsync(_slotId, CancellationToken.None))!;
        slotOfFirst.Status = SlotStatus.Booked;
        slotOfSecond.Status = SlotStatus.Booked;

        await slotsOfFirst.SaveChangesAsync(CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => slotsOfSecond.SaveChangesAsync(CancellationToken.None));
        Assert.IsType<DbUpdateConcurrencyException>(ex.InnerException);
    }

    [Fact]
    public async Task SlotChangedAndSavedTwiceInSameContext_DoesNotThrow()
    {
        // Control: the token is renewed on every save, so one user saving twice in a row is not a conflict.
        await using var db = new AppDbContext(_options);
        var slot = await db.RoomSlots.SingleAsync(s => s.Id == _slotId);

        slot.Status = SlotStatus.Booked;
        await db.SaveChangesAsync();
        slot.Status = SlotStatus.Available;
        await db.SaveChangesAsync();

        await using var check = new AppDbContext(_options);
        Assert.Equal(SlotStatus.Available, (await check.RoomSlots.AsNoTracking().SingleAsync(s => s.Id == _slotId)).Status);
    }
}
