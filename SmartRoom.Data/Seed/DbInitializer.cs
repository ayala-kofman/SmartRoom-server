using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Options;

namespace SmartRoom.Data.Seed;

/// <summary>
/// Runs at server startup: applies migrations, then seeds the time-dependent demo data
/// (users, a one-week booking period with its slots, sample bookings) — only when there are no users yet.
/// Fixed reference data (classes, lessons, rooms, features) comes from HasData in the configurations.
/// </summary>
public class DbInitializer
{
    private const int PeriodLengthDays = 7;

    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly BookingRulesOptions _rules;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(AppDbContext db, TimeProvider time, IOptions<BookingRulesOptions> rules, ILogger<DbInitializer> logger)
    {
        _db = db;
        _time = time;
        _rules = rules.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.Users.AnyAsync(ct))
        {
            _logger.LogInformation("Database already has users; seed skipped.");
            return;
        }

        var nowUtc = _time.GetUtcNow().UtcDateTime;
        var classes = await _db.ClassGroups.ToDictionaryAsync(c => c.Name, ct);

        var secretary = SeedUsers(classes, nowUtc, out var teachers);
        var slots = await SeedPeriodAndSlotsAsync(secretary, nowUtc, ct);
        SeedBookings(slots, teachers, classes, nowUtc);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded demo users, a {Days}-day booking period with {SlotCount} slots, and sample bookings.",
            PeriodLengthDays, slots.Count);
    }

    /// <summary>All ID numbers below are valid (9 digits + check digit). Log in with them in Swagger.</summary>
    private User SeedUsers(Dictionary<string, ClassGroup> classes, DateTime nowUtc, out User[] teachers)
    {
        var secretary = new User { FullName = "שרה המזכירה", IdentityNumber = "100000009", Role = UserRole.Secretary, CreatedAt = nowUtc };

        teachers = new[]
        {
            Teacher("רינה כהן", "200000008", classes["ט'1"], classes["ט'2"]),
            Teacher("מרים לוי", "200000016", classes["י'1"], classes["י'2"], classes["י\"א1"]),
            Teacher("דבורה פרידמן", "200000024", classes["י\"ב1"], classes["ט'2"])
        };

        var students = new[]
        {
            Student("נועה ישראלי", "300000007", classes["ט'1"]),
            Student("תמר אברהם", "300000015", classes["ט'2"]),
            Student("אילה גולד", "300000023", classes["י'1"]),
            Student("שירה מזרחי", "300000031", classes["י\"ב1"])
        };

        _db.Users.Add(secretary);
        _db.Users.AddRange(teachers);
        _db.Users.AddRange(students);
        return secretary;

        User Teacher(string name, string id, params ClassGroup[] taught) => new()
        {
            FullName = name, IdentityNumber = id, Role = UserRole.Teacher, CreatedAt = nowUtc,
            TaughtClassGroups = taught.ToList()
        };

        User Student(string name, string id, ClassGroup classGroup) => new()
        {
            FullName = name, IdentityNumber = id, Role = UserRole.Student, CreatedAt = nowUtc,
            ClassGroup = classGroup
        };
    }

    /// <summary>One week starting tomorrow (Israel time): school days only (no Shabbat), active rooms only (rule 11).</summary>
    private async Task<List<RoomSlot>> SeedPeriodAndSlotsAsync(User secretary, DateTime nowUtc, CancellationToken ct)
    {
        var israelZone = TimeZoneInfo.FindSystemTimeZoneById(_rules.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, israelZone));
        var start = today.AddDays(1);
        var end = start.AddDays(PeriodLengthDays - 1);

        _db.BookingPeriods.Add(new BookingPeriod { StartDate = start, EndDate = end, CreatedByUser = secretary, CreatedAt = nowUtc });

        var roomIds = await _db.Rooms.Where(r => r.Status == RoomStatus.Active).Select(r => r.Id).ToListAsync(ct);
        var lessonIds = await _db.LessonPeriods.OrderBy(p => p.LessonNumber).Select(p => p.Id).ToListAsync(ct);

        var slots = new List<RoomSlot>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
                continue;

            foreach (var roomId in roomIds)
                foreach (var lessonId in lessonIds)
                    slots.Add(new RoomSlot { RoomId = roomId, Date = date, LessonPeriodId = lessonId });
        }

        _db.RoomSlots.AddRange(slots);
        return slots;
    }

    /// <summary>A few bookings on the first two school days; each teacher books only a class she teaches.</summary>
    private void SeedBookings(List<RoomSlot> slots, User[] teachers, Dictionary<string, ClassGroup> classes, DateTime nowUtc)
    {
        var days = slots.Select(s => s.Date).Distinct().OrderBy(d => d).Take(2).ToArray();

        // (teacher, class, room id, day index, lesson period id) — no teacher or class twice at the same time.
        var samples = new (User Teacher, string ClassName, int RoomId, int Day, int LessonPeriodId)[]
        {
            (teachers[0], "ט'1", 7, 0, 3),   // computer lab 203, premium
            (teachers[1], "י'1", 3, 0, 1),   // room 201, premium
            (teachers[2], "י\"ב1", 5, 1, 2), // hall 301, premium
            (teachers[0], "ט'2", 2, 1, 4)    // room 102
        };

        foreach (var (teacher, className, roomId, day, lessonPeriodId) in samples)
        {
            var slot = slots.Single(s => s.RoomId == roomId && s.Date == days[day] && s.LessonPeriodId == lessonPeriodId);
            slot.Status = SlotStatus.Booked;

            _db.Bookings.Add(new Booking
            {
                RoomSlot = slot,
                Teacher = teacher,
                ClassGroup = classes[className],
                Note = "הזמנה לדוגמה",
                CreatedAt = nowUtc
            });
        }
    }
}
