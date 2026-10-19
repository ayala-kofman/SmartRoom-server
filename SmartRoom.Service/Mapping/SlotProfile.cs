using AutoMapper;
using SmartRoom.Core.DTOs.Slots;
using SmartRoom.Core.Entities;

namespace SmartRoom.Service.Mapping;

public class SlotProfile : Profile
{
    public SlotProfile()
    {
        // Flat view of the slot with its room and lesson hour (RoomId maps by name).
        CreateMap<RoomSlot, RoomSlotDto>()
            .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.Room.RoomNumber))
            .ForMember(d => d.RoomName, o => o.MapFrom(s => s.Room.Name))
            .ForMember(d => d.Floor, o => o.MapFrom(s => s.Room.Floor))
            .ForMember(d => d.Quality, o => o.MapFrom(s => s.Room.Quality))
            .ForMember(d => d.LessonNumber, o => o.MapFrom(s => s.LessonPeriod.LessonNumber))
            .ForMember(d => d.StartTime, o => o.MapFrom(s => s.LessonPeriod.StartTime))
            .ForMember(d => d.EndTime, o => o.MapFrom(s => s.LessonPeriod.EndTime))
            .ForMember(d => d.RoomType, o => o.MapFrom((s, _) => RoomProfile.RoomTypeOf(s.Room)))
            .ForMember(d => d.Capacity, o => o.MapFrom((s, _) => RoomProfile.CapacityOf(s.Room)));

        CreateMap<LessonPeriod, LessonPeriodDto>();

        CreateMap<BookingPeriod, BookingPeriodDto>()
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedByUser.FullName));

        // The service sets who opened the period and when.
        CreateMap<CreateBookingPeriodRequest, BookingPeriod>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedByUserId, o => o.Ignore())
            .ForMember(d => d.CreatedByUser, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
    }
}
