using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Exceptions;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Models;
using SmartRoom.Core.Options;
using SmartRoom.Core.Results;

namespace SmartRoom.Service.Services;

/// <summary>
/// The heart of the system: booking and cancelling room slots (rules in chapter 5, flow in chapter 7).
/// The slot's Version token closes the window between the business checks and the save.
/// </summary>
public class BookingService : IBookingService
{
    private const string SlotTakenMessage =
        "The room was just booked by another teacher. Please choose another room.";

    private readonly IBookingRepository _bookings;
    private readonly IRoomSlotRepository _slots;
    private readonly IUserRepository _users;
    private readonly IBookingWindowService _window;
    private readonly TimeProvider _time;
    private readonly IMapper _mapper;
    private readonly PagingOptions _paging;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookings,
        IRoomSlotRepository slots,
        IUserRepository users,
        IBookingWindowService window,
        TimeProvider time,
        IMapper mapper,
        IOptions<PagingOptions> paging,
        ILogger<BookingService> logger)
    {
        _bookings = bookings;
        _slots = slots;
        _users = users;
        _window = window;
        _time = time;
        _mapper = mapper;
        _paging = paging.Value;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> CreateAsync(CreateBookingRequest request, CurrentUser currentUser, CancellationToken ct)
    {
        // The controller already allows teachers only; checked again so the rule lives in the service.
        if (currentUser.Role != UserRole.Teacher)
            return Result<BookingDto>.Forbidden("Only a teacher can book a room.");

        var slotId = request.RoomSlotId!.Value;
        var classGroupId = request.ClassGroupId!.Value;

        // 1. Tracked, so EF checks the Version that was read when it saves.
        var slot = await _slots.GetForUpdateAsync(slotId, ct);
        if (slot is null)
            return Result<BookingDto>.NotFound($"Slot {slotId} was not found.");

        // 2. Business checks. Date and permission come first, so a past slot is not reported as "taken".
        var bookableDate = _window.GetBookableDate();
        if (slot.Date != bookableDate)
            return Result<BookingDto>.ValidationError($"Only slots of {bookableDate:yyyy-MM-dd} can be booked now.");

        if (!await _users.TeacherTeachesClassAsync(currentUser.Id, classGroupId, ct))
            return Result<BookingDto>.Forbidden("You can book only for a class you teach.");

        if (slot.Status != SlotStatus.Available)
        {
            _logger.LogWarning("Booking conflict: slot {SlotId} is already booked; teacher {TeacherId} was refused.",
                slotId, currentUser.Id);
            return Result<BookingDto>.Conflict(SlotTakenMessage, ErrorCodes.SlotTaken);
        }

        if (slot.Room.Status != RoomStatus.Active)
            return Result<BookingDto>.Conflict("The room is not active and cannot be booked.", ErrorCodes.RoomNotActive);

        if (await _bookings.TeacherHasBookingAtAsync(currentUser.Id, slot.Date, slot.LessonPeriodId, ct))
            return Result<BookingDto>.Conflict("You already have a room booked for this lesson.", ErrorCodes.TeacherBusy);

        if (await _bookings.ClassHasBookingAtAsync(classGroupId, slot.Date, slot.LessonPeriodId, ct))
            return Result<BookingDto>.Conflict("This class already has a room booked for this lesson.", ErrorCodes.ClassBusy);

        // 3. The slot becomes booked and a booking is added.
        var booking = _mapper.Map<Booking>(request);
        booking.RoomSlot = slot;
        booking.TeacherId = currentUser.Id;
        booking.CreatedAt = _time.GetUtcNow().UtcDateTime;
        booking.IsActive = true;
        slot.Status = SlotStatus.Booked;
        await _bookings.AddAsync(booking, ct);

        // 4-6. One save = one transaction; a lost race throws instead of updating 0 rows silently.
        try
        {
            await _bookings.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            // 7. CorrelationId is added by the logging scope of the request (stage 7).
            _logger.LogWarning(
                "Booking conflict: slot {SlotId} was taken before teacher {TeacherId} could save.",
                slotId, currentUser.Id);
            return Result<BookingDto>.Conflict(SlotTakenMessage, ErrorCodes.SlotTaken);
        }

        _logger.LogInformation(
            "Booking {BookingId} created: slot {SlotId} (room {RoomId}, {Date:yyyy-MM-dd}, lesson {LessonPeriodId}) by teacher {TeacherId} for class {ClassGroupId}.",
            booking.Id, slotId, slot.RoomId, slot.Date, slot.LessonPeriodId, currentUser.Id, classGroupId);

        // Reloaded to get the teacher and class names for the DTO.
        var created = await _bookings.GetByIdAsync(booking.Id, ct);
        return Result<BookingDto>.Created(_mapper.Map<BookingDto>(created));
    }

    public async Task<Result<PagedResult<BookingDto>>> GetPagedAsync(BookingQuery query, CurrentUser currentUser, CancellationToken ct)
    {
        // Who sees what comes from the token, never from the query.
        int? teacherId = null;
        int? classGroupId = null;

        switch (currentUser.Role)
        {
            case UserRole.Teacher:
                teacherId = currentUser.Id;
                break;

            case UserRole.Student:
                if (currentUser.ClassGroupId is null)
                    return Result<PagedResult<BookingDto>>.Forbidden("Your account is not assigned to a class.");

                // A student sees her class's bookings for one day; today when no date is sent (rule 13).
                classGroupId = currentUser.ClassGroupId;
                query = new BookingQuery
                {
                    Date = query.Date ?? _window.GetToday(),
                    Page = query.Page,
                    PageSize = query.PageSize
                };
                break;
        }

        var page = await _bookings.GetPagedAsync(query, _paging.ResolvePageSize(query.PageSize), teacherId, classGroupId, ct);
        _logger.LogDebug(
            "Bookings query by {Role}: date {Date:yyyy-MM-dd}, from {FromDate:yyyy-MM-dd}, to {ToDate:yyyy-MM-dd}, teacher {TeacherId}, class {ClassGroupId}, page {Page}: {TotalCount} found.",
            currentUser.Role, query.Date, query.FromDate, query.ToDate, teacherId, classGroupId, page.Page, page.TotalCount);
        return Result<PagedResult<BookingDto>>.Success(_mapper.MapPage<Booking, BookingDto>(page));
    }

    public async Task<Result<BookingDto>> GetByIdAsync(int id, CurrentUser currentUser, CancellationToken ct)
    {
        var booking = await _bookings.GetByIdAsync(id, ct);
        if (booking is null)
            return Result<BookingDto>.NotFound($"Booking {id} was not found.");

        if (!CanManage(booking, currentUser))
            return Result<BookingDto>.Forbidden("You can view only your own bookings.");

        return Result<BookingDto>.Success(_mapper.Map<BookingDto>(booking));
    }

    public async Task<Result> CancelAsync(int id, CurrentUser currentUser, CancellationToken ct)
    {
        // Tracked with its slot: the slot goes back to available, checked by the same Version token.
        var booking = await _bookings.GetForUpdateAsync(id, ct);
        if (booking is null)
            return Result.NotFound($"Booking {id} was not found.");

        if (!CanManage(booking, currentUser))
            return Result.Forbidden("You can cancel only your own bookings.");

        if (!booking.IsActive)
            return Result.Conflict("The booking is already cancelled.", ErrorCodes.AlreadyCancelled);

        // Rule 7: until the lesson starts (also on the same day), for anyone. A lesson that began or ended stays as history.
        if (_window.HasStarted(booking.RoomSlot.Date, booking.RoomSlot.LessonPeriod.StartTime))
            return Result.ValidationError("A booking can be cancelled only before the lesson starts.");

        booking.IsActive = false;
        booking.CancelledAt = _time.GetUtcNow().UtcDateTime;
        booking.RoomSlot.Status = SlotStatus.Available;

        try
        {
            await _bookings.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning(
                "Cancel conflict: slot {SlotId} of booking {BookingId} was changed before user {UserId} could save.",
                booking.RoomSlotId, id, currentUser.Id);
            return Result.Conflict(
                "The booking was changed by someone else a moment ago. Please refresh and try again.",
                ErrorCodes.ConcurrentChange);
        }

        _logger.LogInformation("Booking {BookingId} cancelled (slot {SlotId} is available again) by user {UserId} with role {Role}.",
            id, booking.RoomSlotId, currentUser.Id, currentUser.Role);
        return Result.Success();
    }

    /// <summary>The secretary manages every booking; a teacher only her own; a student none.</summary>
    private static bool CanManage(Booking booking, CurrentUser currentUser) =>
        currentUser.Role == UserRole.Secretary
        || (currentUser.Role == UserRole.Teacher && booking.TeacherId == currentUser.Id);
}
