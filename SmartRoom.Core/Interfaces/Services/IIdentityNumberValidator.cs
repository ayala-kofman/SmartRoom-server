namespace SmartRoom.Core.Interfaces.Services;

public interface IIdentityNumberValidator
{
    /// <summary>True if the value is 9 digits with a valid Israeli ID check digit.</summary>
    bool IsValid(string identityNumber);
}
