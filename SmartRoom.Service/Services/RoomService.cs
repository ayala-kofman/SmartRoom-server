using AutoMapper;
using Microsoft.Extensions.Options;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Options;
using SmartRoom.Core.Results;
using SmartRoom.Service.Mapping;

namespace SmartRoom.Service.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _rooms;
    private readonly IRoomSlotRepository _slots;
    private readonly IBookingPeriodRepository _periods;
    private readonly IReferenceDataRepository _referenceData;
    private readonly IBookingWindowService _window;
    private readonly IMapper _mapper;
    private readonly PagingOptions _paging;

    public RoomService(
        IRoomRepository rooms,
        IRoomSlotRepository slots,
        IBookingPeriodRepository periods,
        IReferenceDataRepository referenceData,
        IBookingWindowService window,
        IMapper mapper,
        IOptions<PagingOptions> paging)
    {
        _rooms = rooms;
        _slots = slots;
        _periods = periods;
        _referenceData = referenceData;
        _window = window;
        _mapper = mapper;
        _paging = paging.Value;
    }

    public async Task<Result<PagedResult<RoomDto>>> GetPagedAsync(RoomQuery query, CancellationToken ct)
    {
        var page = await _rooms.GetPagedAsync(query, _paging.ResolvePageSize(query.PageSize), ct);
        return Result<PagedResult<RoomDto>>.Success(_mapper.MapPage<Room, RoomDto>(page));
    }

    public async Task<Result<RoomDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(id, ct);
        return room is null
            ? Result<RoomDto>.NotFound($"Room {id} was not found.")
            : Result<RoomDto>.Success(_mapper.Map<RoomDto>(room));
    }

    public async Task<Result<RoomDto>> CreateAsync(CreateRoomRequest request, CancellationToken ct)
    {
        var capacityError = CheckCapacity(request.RoomType!.Value, request.SeatCount, request.ComputerCount);
        if (capacityError is not null)
            return Result<RoomDto>.ValidationError(capacityError);

        if (await _rooms.RoomNumberExistsAsync(request.RoomNumber, null, ct))
            return Result<RoomDto>.Conflict($"Room number {request.RoomNumber} already exists.");

        var features = await _referenceData.GetFeaturesByIdsAsync(request.FeatureIds.Distinct(), ct);
        if (features.Count != request.FeatureIds.Distinct().Count())
            return Result<RoomDto>.ValidationError("One or more feature ids do not exist.");

        Room room = request.RoomType == RoomType.Computer
            ? _mapper.Map<ComputerRoom>(request)
            : _mapper.Map<RegularRoom>(request);
        room.Features = features.ToList();

        await _rooms.AddAsync(room, ct);
        await AddSlotsForOpenPeriodsAsync(room, ct);

        // One save: the room and its slots are created together or not at all.
        await _rooms.SaveChangesAsync(ct);

        return Result<RoomDto>.Created(_mapper.Map<RoomDto>(room));
    }

    public async Task<Result> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken ct)
    {
        var room = await _rooms.GetForUpdateAsync(id, ct);
        if (room is null)
            return Result.NotFound($"Room {id} was not found.");

        var capacityError = CheckCapacity(RoomProfile.RoomTypeOf(room), request.SeatCount, request.ComputerCount);
        if (capacityError is not null)
            return Result.ValidationError(capacityError);

        if (await _rooms.RoomNumberExistsAsync(request.RoomNumber, id, ct))
            return Result.Conflict($"Room number {request.RoomNumber} already exists.");

        var features = await _referenceData.GetFeaturesByIdsAsync(request.FeatureIds.Distinct(), ct);
        if (features.Count != request.FeatureIds.Distinct().Count())
            return Result.ValidationError("One or more feature ids do not exist.");

        var wasActive = room.Status == RoomStatus.Active;

        _mapper.Map(request, room, typeof(UpdateRoomRequest), room.GetType());
        room.Features.Clear();
        foreach (var feature in features)
            room.Features.Add(feature);

        // A room that becomes active again gets its missing slots right away, like a new room.
        if (!wasActive && room.Status == RoomStatus.Active)
            await AddSlotsForOpenPeriodsAsync(room, ct);

        await _rooms.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var room = await _rooms.GetForUpdateAsync(id, ct);
        if (room is null)
            return Result.NotFound($"Room {id} was not found.");

        if (room.Status != RoomStatus.Closed)
            return Result.Conflict("Only a closed room can be deleted. Close the room first.");

        // Bookings are history, even past or cancelled ones; a room with any booking stays closed (rule 9).
        if (await _rooms.HasAnyBookingsAsync(id, ct))
            return Result.Conflict("The room has bookings and cannot be deleted. It stays closed.");

        // Its slots (all without bookings) are deleted with it by cascade.
        _rooms.Remove(room);
        await _rooms.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// Rule 12: slots for every open booking period, from tomorrow on (nothing changes today or in the past).
    /// Existing slots are skipped, so a room returning from maintenance gets only what it is missing.
    /// </summary>
    private async Task AddSlotsForOpenPeriodsAsync(Room room, CancellationToken ct)
    {
        var tomorrow = _window.GetToday().AddDays(1);
        var periods = await _periods.GetOpenFromAsync(tomorrow, ct);
        if (periods.Count == 0)
            return;

        var lessons = await _referenceData.GetLessonPeriodsAsync(ct);
        var existing = await _slots.GetExistingKeysAsync(tomorrow, periods.Max(p => p.EndDate), ct);

        var newSlots = new List<RoomSlot>();
        foreach (var period in periods)
        {
            var from = period.StartDate > tomorrow ? period.StartDate : tomorrow;
            foreach (var date in ServiceHelpers.SchoolDays(from, period.EndDate))
            {
                foreach (var lesson in lessons)
                {
                    // Periods may overlap; the set also guards against adding the same slot twice here.
                    if (existing.Add((room.Id, date, lesson.Id)))
                        newSlots.Add(new RoomSlot { Room = room, Date = date, LessonPeriodId = lesson.Id });
                }
            }
        }

        await _slots.AddRangeAsync(newSlots, ct);
    }

    private static string? CheckCapacity(RoomType roomType, int? seatCount, int? computerCount) => roomType switch
    {
        RoomType.Regular when seatCount is null => "SeatCount is required for a regular room.",
        RoomType.Computer when computerCount is null => "ComputerCount is required for a computer room.",
        _ => null
    };
}
