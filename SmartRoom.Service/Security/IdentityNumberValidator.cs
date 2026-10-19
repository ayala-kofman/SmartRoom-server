using SmartRoom.Core.Interfaces.Services;

namespace SmartRoom.Service.Security;

/// <summary>Israeli ID number check (Luhn-style): 9 digits whose weighted digit sum is divisible by 10.</summary>
public class IdentityNumberValidator : IIdentityNumberValidator
{
    public bool IsValid(string identityNumber)
    {
        if (identityNumber is null || identityNumber.Length != 9 || !identityNumber.All(char.IsAsciiDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < identityNumber.Length; i++)
        {
            // Digits are multiplied by 1, 2, 1, 2, ...; a two-digit product counts as the sum of its digits.
            var product = (identityNumber[i] - '0') * (i % 2 + 1);
            sum += product > 9 ? product - 9 : product;
        }

        return sum % 10 == 0;
    }
}
