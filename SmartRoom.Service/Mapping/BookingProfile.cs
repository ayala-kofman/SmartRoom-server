using AutoMapper;
using SmartRoom.Core.DTOs.Bookings;
using SmartRoom.Core.Entities;

namespace SmartRoom.Service.Mapping;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        // Needs slot → room, slot → lesson period, teacher and class loaded (BookingRepository does that).
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.RoomName, o => o.MapFrom(s => s.RoomSlot.Room.Name))
            .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.RoomSlot.Room.RoomNumber))
            .ForMember(d => d.Floor, o => o.MapFrom(s => s.RoomSlot.Room.Floor))
            .ForMember(d => d.Date, o => o.MapFrom(s => s.RoomSlot.Date))
            .ForMember(d => d.LessonNumber, o => o.MapFrom(s => s.RoomSlot.LessonPeriod.LessonNumber))
            .ForMember(d => d.StartTime, o => o.MapFrom(s => s.RoomSlot.LessonPeriod.StartTime))
            .ForMember(d => d.EndTime, o => o.MapFrom(s => s.RoomSlot.LessonPeriod.EndTime))
            .ForMember(d => d.TeacherName, o => o.MapFrom(s => s.Teacher.FullName))
            .ForMember(d => d.ClassGroupName, o => o.MapFrom(s => s.ClassGroup.Name));

        // The teacher comes from the token and the time from TimeProvider, both set by the service.
        CreateMap<CreateBookingRequest, Booking>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.RoomSlot, o => o.Ignore())
            .ForMember(d => d.TeacherId, o => o.Ignore())
            .ForMember(d => d.Teacher, o => o.Ignore())
            .ForMember(d => d.ClassGroup, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CancelledAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());
    }
}
