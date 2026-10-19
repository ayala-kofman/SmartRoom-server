using AutoMapper;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;

namespace SmartRoom.Service.Mapping;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDto>()
            .ForMember(d => d.RoomType, o => o.MapFrom((s, _) => RoomTypeOf(s)))
            .ForMember(d => d.Capacity, o => o.MapFrom((s, _) => CapacityOf(s)))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.Features.Select(f => f.Name)));

        CreateMap<Feature, FeatureDto>();

        // Request → entity. The service picks the subclass by RoomType and attaches the features itself.
        CreateMap<CreateRoomRequest, RegularRoom>().IgnoreRoomKeysAndLinks().ForMember(d => d.Status, o => o.Ignore());
        CreateMap<CreateRoomRequest, ComputerRoom>().IgnoreRoomKeysAndLinks().ForMember(d => d.Status, o => o.Ignore());

        // Update maps onto the existing tracked room; use its runtime type:
        // mapper.Map(request, room, typeof(UpdateRoomRequest), room.GetType()).
        CreateMap<UpdateRoomRequest, RegularRoom>().IgnoreRoomKeysAndLinks();
        CreateMap<UpdateRoomRequest, ComputerRoom>().IgnoreRoomKeysAndLinks();
    }

    internal static RoomType RoomTypeOf(Room room) =>
        room is ComputerRoom ? RoomType.Computer : RoomType.Regular;

    /// <summary>Seats for a regular room, computers for a computer room.</summary>
    internal static int CapacityOf(Room room) => room switch
    {
        RegularRoom regular => regular.SeatCount,
        ComputerRoom computer => computer.ComputerCount,
        _ => 0
    };
}

internal static class RoomMappingExtensions
{
    /// <summary>Id is set by the database; slots and features are handled by the service, never from a request.</summary>
    public static IMappingExpression<TSource, TRoom> IgnoreRoomKeysAndLinks<TSource, TRoom>(
        this IMappingExpression<TSource, TRoom> map)
        where TRoom : Room =>
        map.ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Slots, o => o.Ignore())
            .ForMember(d => d.Features, o => o.Ignore());
}
