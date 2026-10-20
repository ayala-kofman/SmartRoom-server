using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartRoom.Core.DTOs.Auth;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Results;
using SmartRoom.Service.Security;
using SmartRoom.Service.Services;

namespace SmartRoom.Tests.Services;

public class AuthServiceTests
{
    // Valid check digit; the same IDs the seed uses.
    private const string ValidId = "300000007";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IReferenceDataRepository> _referenceData = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var validator = new IdentityNumberValidator();
        // Registration goes through the real UserService, so its duplicate-ID check is what is tested.
        var userService = new UserService(
            _users.Object, _referenceData.Object, validator, TestHelpers.Clock(),
            TestHelpers.Mapper(), TestHelpers.Paging(), NullLogger<UserService>.Instance);
        _service = new AuthService(userService, _users.Object, validator, _tokens.Object, NullLogger<AuthService>.Instance);
    }

    [Theory]
    [InlineData("300000008")] // wrong check digit
    [InlineData("12345")]     // too short
    [InlineData("abcdefghi")] // not digits
    public async Task LoginAsync_InvalidIdNumber_ReturnsUnauthorizedWithoutLookingUpUser(string identityNumber)
    {
        var result = await _service.LoginAsync(new LoginRequest { IdentityNumber = identityNumber }, CancellationToken.None);

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
        _users.Verify(u => u.GetByIdentityNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokens.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidButUnknownIdNumber_ReturnsUnauthorizedWithSameMessage()
    {
        _users.Setup(u => u.GetByIdentityNumberAsync(ValidId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var unknown = await _service.LoginAsync(new LoginRequest { IdentityNumber = ValidId }, CancellationToken.None);
        var invalid = await _service.LoginAsync(new LoginRequest { IdentityNumber = "300000008" }, CancellationToken.None);

        // The same message, so the response does not reveal which ID numbers exist.
        Assert.Equal(ResultStatus.Unauthorized, unknown.Status);
        Assert.Equal(invalid.Error, unknown.Error);
        _tokens.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Teacher_ReturnsTokenAndTaughtClasses()
    {
        var teacher = new User
        {
            Id = 2, FullName = "Teacher", IdentityNumber = "200000008", Role = Core.Enums.UserRole.Teacher,
            TaughtClassGroups = { new ClassGroup { Id = 3, Name = "י'1", Grade = 10 }, new ClassGroup { Id = 1, Name = "ט'1", Grade = 9 } }
        };
        _users.Setup(u => u.GetByIdentityNumberAsync("200000008", It.IsAny<CancellationToken>())).ReturnsAsync(teacher);
        _tokens.Setup(t => t.Generate(teacher)).Returns(("token", DateTime.UtcNow.AddHours(1)));

        var result = await _service.LoginAsync(new LoginRequest { IdentityNumber = "200000008" }, CancellationToken.None);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("token", result.Value!.Token);
        Assert.Equal(new[] { 1, 3 }, result.Value.TaughtClassGroups.Select(c => c.Id)); // ordered by grade
    }

    [Fact]
    public async Task RegisterAsync_IdNumberAlreadyExists_ReturnsConflictAndDoesNotAdd()
    {
        _referenceData.Setup(r => r.GetClassGroupsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassGroup> { new() { Id = 1, Name = "ט1" } });
        _users.Setup(u => u.IdentityNumberExistsAsync(ValidId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.RegisterAsync(
            new RegisterRequest { FullName = "New Student", IdentityNumber = ValidId, ClassGroupId = 1 }, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.IdentityNumberTaken, result.ErrorCode);
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
