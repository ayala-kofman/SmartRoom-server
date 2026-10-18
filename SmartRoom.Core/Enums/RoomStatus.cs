namespace SmartRoom.Core.Enums;

/// <summary>Operational status of a room. Free/booked is a property of a slot, not of the room.</summary>
public enum RoomStatus
{
    Active = 1,
    UnderMaintenance = 2,
    Closed = 3
}
