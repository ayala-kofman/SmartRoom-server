using System.ComponentModel.DataAnnotations;
using SmartRoom.Core.DTOs.Common;
using SmartRoom.Core.Enums;

namespace SmartRoom.Core.DTOs.Users;

/// <summary>Filters for GET api/users. Every filter is optional.</summary>
public class UserQuery : PagingQuery
{
    [EnumDataType(typeof(UserRole))]
    public UserRole? Role { get; set; }
}
