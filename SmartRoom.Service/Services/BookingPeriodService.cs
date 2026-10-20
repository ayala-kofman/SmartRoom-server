using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

public class BookingPeriodService : IBookingPeriodService
{
    private readonly IBookingPeriodRepository _periods;
    private readonly IRoomRepository _rooms;
    private readonly IRoomSlotRepository _slots;
    private readonly IReferenceDataRepository _referenceData;
    private readonly IBookingWindowService _window;
    private readonly TimeProvider _time;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingPeriodService> _logger;

    public BookingPeriodService(
        IBookingPeriodRepository periods,
        IRoomRepository rooms,
        IRoomSlotRepository slots,
        IReferenceDataRepository referenceData,
        IBookingWindowService window,
        TimeProvider time,
        IMapper mapper,
        ILogger<BookingPeriodService> logger)
    {
        _periods = periods;
        _rooms = rooms;
        _slots = slots;
        _referenceData = referenceData;
        _window = window;
        _time = time;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<BookingPeriodDto>>> GetAllAsync(CancellationToken ct)
    {
        var periods = await _periods.GetAllAsync(ct);
        return Result<IReadOnlyList<BookingPeriodDto>>.Success(_mapper.Map<List<BookingPeriodDto>>(periods));
    }

    public async Task<Result<BookingPeriodDto>> CreateAsync(
        CreateBookingPeriodRequest request, CurrentUser currentUser, CancellationToken ct)
    {
        var start = request.StartDate!.Value;
        var end = request.EndDate!.Value;

        if (end < start)
            return Result<BookingPeriodDto>.ValidationError("EndDate cannot be before StartDate.");

        // No change today or in the past.
        if (start <= _window.GetToday())
            return Result<BookingPeriodDto>.ValidationError("A booking period must start tomorrow or later.");

        var period = _mapper.Map<BookingPeriod>(request);
        period.CreatedByUserId = currentUser.Id;
        period.CreatedAt = _time.GetUtcNow().UtcDateTime;
        await _periods.AddAsync(period, ct);

        // Rule 11: school days only, active rooms only, without duplicating slots of an overlapping period.
        var roomIds = await _rooms.GetActiveRoomIdsAsync(ct);
        var lessons = await _referenceData.GetLessonPeriodsAsync(ct);
        var existing = await _slots.GetExistingKeysAsync(start, end, ct);

        var newSlots = new List<RoomSlot>();
        foreach (var date in ServiceHelpers.SchoolDays(start, end))
        {
            foreach (var roomId in roomIds)
            {
                foreach (var lesson in lessons)
                {
                    if (!existing.Contains((roomId, date, lesson.Id)))
                        newSlots.Add(new RoomSlot { RoomId = roomId, Date = date, LessonPeriodId = lesson.Id });
                }
            }
        }

        await _slots.AddRangeAsync(newSlots, ct);

        // One save: the period and all its slots, in one transaction.
        await _periods.SaveChangesAsync(ct);
        _logger.LogInformation("Booking period {PeriodId} ({StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}) opened by user {UserId} with {SlotCount} new slots.",
            period.Id, start, end, currentUser.Id, newSlots.Count);

        // Reload so the DTO has the name of the user who opened the period.
        var created = (await _periods.GetAllAsync(ct)).First(p => p.Id == period.Id);
        return Result<BookingPeriodDto>.Created(_mapper.Map<BookingPeriodDto>(created));
    }
}
