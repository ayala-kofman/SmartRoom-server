using AutoMapper;
using SmartRoom.Core.DTOs.Users;
using SmartRoom.Core.Entities;

namespace SmartRoom.Service.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // UserDto has no IdentityNumber, so the ID number never leaves the server.
        CreateMap<User, UserDto>()
            .ForMember(d => d.ClassGroupName, o => o.MapFrom(s => s.ClassGroup != null ? s.ClassGroup.Name : null))
            .ForMember(d => d.TaughtClassGroups, o => o.MapFrom(s => s.TaughtClassGroups.Select(c => c.Name)));

        CreateMap<ClassGroup, ClassGroupDto>();

        // The service rejects Role = Secretary and attaches the taught classes (ClassGroupIds) itself.
        // Open registration (RegisterRequest) goes through the same path as a CreateUserRequest with Role = Student.
        CreateMap<CreateUserRequest, User>()
            .IgnoreUserKeysAndLinks();
    }
}

internal static class UserMappingExtensions
{
    public static IMappingExpression<TSource, User> IgnoreUserKeysAndLinks<TSource>(this IMappingExpression<TSource, User> map) =>
        map.ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.ClassGroup, o => o.Ignore())
            .ForMember(d => d.TaughtClassGroups, o => o.Ignore())
            .ForMember(d => d.Bookings, o => o.Ignore());
}
