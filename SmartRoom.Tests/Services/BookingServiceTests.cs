using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Exceptions;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;
using SmartRoom.Service.Services;

namespace SmartRoom.Tests.Services;

public class BookingServiceTests
{
    private const int TeacherId = 10;
    private const int OtherTeacherId = 11;
    private const int ClassGroupId = 3;
    private const int SlotId = 100;

    private static readonly CurrentUser Teacher = new(TeacherId, UserRole.Teacher, null);

    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IRoomSlotRepository> _slots = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        var clock = TestHelpers.Clock();
        _service = new BookingService(
            _bookings.Object, _slots.Object, _users.Object, TestHelpers.Window(clock), clock,
            TestHelpers.Mapper(), TestHelpers.Paging(), NullLogger<BookingService>.Instance);

        // Default: the teacher teaches the class and has nothing else booked at that lesson.
        _users.Setup(u => u.TeacherTeachesClassAsync(TeacherId, ClassGroupId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private static RoomSlot Slot(
        DateOnly date, SlotStatus status = SlotStatus.Available, RoomStatus roomStatus = RoomStatus.Active, TimeOnly? startTime = null) => new()
    {
        Id = SlotId,
        RoomId = 1,
        Room = new RegularRoom { Id = 1, RoomNumber = "101", Name = "Room 101", Status = roomStatus, SeatCount = 30 },
        Date = date,
        LessonPeriodId = 2,
        LessonPeriod = new LessonPeriod { Id = 2, LessonNumber = 2, StartTime = startTime ?? new TimeOnly(8, 50), EndTime = (startTime ?? new TimeOnly(8, 50)).AddMinutes(45) },
        Status = status
    };

    private static CreateBookingRequest Request() => new() { RoomSlotId = SlotId, ClassGroupId = ClassGroupId };

    private void GivenSlot(RoomSlot slot) =>
        _slots.Setup(s => s.GetForUpdateAsync(SlotId, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

    // ---- Create ----

    [Fact]
    public async Task CreateAsync_AvailableSlotAndTeacherTeachesClass_ReturnsCreatedBooksSlotAndSavesOnce()
    {
        // Arrange
        var slot = Slot(TestHelpers.Tomorrow);
        GivenSlot(slot);
        Booking? added = null;
        _bookings.Setup(b => b.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, _) => added = b);
        _bookings.Setup(b => b.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => added);

        // Act
        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        // Assert
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.Equal(SlotStatus.Booked, slot.Status);
        Assert.NotNull(added);
        Assert.Equal(TeacherId, added!.TeacherId);
        Assert.Equal(ClassGroupId, added.ClassGroupId);
        Assert.True(added.IsActive);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SlotAlreadyBooked_ReturnsConflictAndDoesNotSave()
    {
        GivenSlot(Slot(TestHelpers.Tomorrow, SlotStatus.Booked));

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.SlotTaken, result.ErrorCode);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SaveThrowsConcurrencyConflict_ReturnsConflictWithSlotTakenMessage()
    {
        // Arrange: the checks pass, but another teacher saved the same slot first.
        GivenSlot(Slot(TestHelpers.Tomorrow));
        _bookings.Setup(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("The data was changed by another user."));

        // Act
        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        // Assert
        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.SlotTaken, result.ErrorCode);
        Assert.Equal("The room was just booked by another teacher. Please choose another room.", result.Error);
    }

    [Fact]
    public async Task CreateAsync_TeacherDoesNotTeachClass_ReturnsForbidden()
    {
        GivenSlot(Slot(TestHelpers.Tomorrow));
        _users.Setup(u => u.TeacherTeachesClassAsync(TeacherId, ClassGroupId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Forbidden, result.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)] // yesterday
    [InlineData(0)]  // today
    [InlineData(2)]  // the day after tomorrow
    public async Task CreateAsync_SlotNotTomorrow_ReturnsValidationError(int daysFromToday)
    {
        GivenSlot(Slot(TestHelpers.Today.AddDays(daysFromToday)));

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.ValidationError, result.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RoomUnderMaintenance_ReturnsConflict()
    {
        GivenSlot(Slot(TestHelpers.Tomorrow, roomStatus: RoomStatus.UnderMaintenance));

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.RoomNotActive, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_TeacherAlreadyHasRoomAtSameLesson_ReturnsConflict()
    {
        GivenSlot(Slot(TestHelpers.Tomorrow));
        _bookings.Setup(b => b.TeacherHasBookingAtAsync(TeacherId, TestHelpers.Tomorrow, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.TeacherBusy, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_SlotDoesNotExist_ReturnsNotFound()
    {
        _slots.Setup(s => s.GetForUpdateAsync(SlotId, It.IsAny<CancellationToken>())).ReturnsAsync((RoomSlot?)null);

        var result = await _service.CreateAsync(Request(), Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    // ---- Cancel ----

    private Booking GivenBooking(int teacherId, DateOnly lessonDate, TimeOnly? startTime = null)
    {
        var booking = new Booking
        {
            Id = 7,
            TeacherId = teacherId,
            RoomSlotId = SlotId,
            RoomSlot = Slot(lessonDate, SlotStatus.Booked, startTime: startTime),
            ClassGroupId = ClassGroupId,
            IsActive = true
        };
        _bookings.Setup(b => b.GetForUpdateAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        return booking;
    }

    [Fact]
    public async Task CancelAsync_BookingOfAnotherTeacher_ReturnsForbiddenAndDoesNotSave()
    {
        GivenBooking(OtherTeacherId, TestHelpers.Tomorrow);

        var result = await _service.CancelAsync(7, Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Forbidden, result.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // The clock is Wednesday 10:00 (Israel).
    [Theory]
    [InlineData(0, 8, 50)]  // today, the lesson started at 08:50
    [InlineData(0, 10, 0)]  // today, the lesson starts right now
    [InlineData(-1, 12, 0)] // yesterday
    public async Task CancelAsync_LessonAlreadyStartedOrInThePast_ReturnsValidationError(int daysFromToday, int hour, int minute)
    {
        GivenBooking(TeacherId, TestHelpers.Today.AddDays(daysFromToday), new TimeOnly(hour, minute));

        var result = await _service.CancelAsync(7, Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.ValidationError, result.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_LessonLaterToday_SucceedsAndFreesSlot()
    {
        var booking = GivenBooking(TeacherId, TestHelpers.Today, new TimeOnly(12, 30));

        var result = await _service.CancelAsync(7, Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(booking.IsActive);
        Assert.Equal(SlotStatus.Available, booking.RoomSlot.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_OwnBookingForTomorrow_SucceedsAndFreesSlot()
    {
        var booking = GivenBooking(TeacherId, TestHelpers.Tomorrow);

        var result = await _service.CancelAsync(7, Teacher, CancellationToken.None);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(booking.IsActive);
        Assert.Equal(SlotStatus.Available, booking.RoomSlot.Status);
        _bookings.Verify(b => b.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- Role filter ----

    [Fact]
    public async Task GetPagedAsync_Student_AsksOnlyForOwnClassForToday()
    {
        // Arrange: the student tries to see a teacher's bookings by sending other filters.
        var student = new CurrentUser(50, UserRole.Student, ClassGroupId);
        _bookings.Setup(b => b.GetPagedAsync(It.IsAny<BookingQuery>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Booking>());

        // Act
        var result = await _service.GetPagedAsync(
            new BookingQuery { FromDate = TestHelpers.Today, ToDate = TestHelpers.Today.AddDays(30) }, student, CancellationToken.None);

        // Assert: restricted to her class, no teacher filter, a single day (today), the range is ignored.
        Assert.Equal(ResultStatus.Success, result.Status);
        _bookings.Verify(b => b.GetPagedAsync(
            It.Is<BookingQuery>(q => q.Date == TestHelpers.Today && q.FromDate == null && q.ToDate == null),
            It.IsAny<int>(), null, ClassGroupId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_Teacher_AsksOnlyForOwnBookings()
    {
        _bookings.Setup(b => b.GetPagedAsync(It.IsAny<BookingQuery>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Booking>());

        await _service.GetPagedAsync(new BookingQuery(), Teacher, CancellationToken.None);

        _bookings.Verify(b => b.GetPagedAsync(It.IsAny<BookingQuery>(), It.IsAny<int>(), TeacherId, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
