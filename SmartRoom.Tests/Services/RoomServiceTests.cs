using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartRoom.Core.DTOs.Rooms;
using SmartRoom.Core.Entities;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Interfaces.Repositories;
using SmartRoom.Core.Results;
using SmartRoom.Service.Services;

namespace SmartRoom.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<IRoomRepository> _rooms = new();
    private readonly Mock<IRoomSlotRepository> _slots = new();
    private readonly Mock<IBookingPeriodRepository> _periods = new();
    private readonly Mock<IReferenceDataRepository> _referenceData = new();
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        var clock = TestHelpers.Clock();
        _service = new RoomService(
            _rooms.Object, _slots.Object, _periods.Object, _referenceData.Object, TestHelpers.Window(clock),
            TestHelpers.Mapper(), TestHelpers.Paging(), NullLogger<RoomService>.Instance);
    }

    private void GivenRoom(RoomStatus status) =>
        _rooms.Setup(r => r.GetForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegularRoom { Id = 5, RoomNumber = "105", Name = "Room 105", Status = status, SeatCount = 30 });

    [Theory]
    [InlineData(RoomStatus.Active)]
    [InlineData(RoomStatus.UnderMaintenance)]
    public async Task DeleteAsync_RoomNotClosed_ReturnsConflictAndDoesNotRemove(RoomStatus status)
    {
        GivenRoom(status);

        var result = await _service.DeleteAsync(5, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.RoomNotClosed, result.ErrorCode);
        _rooms.Verify(r => r.Remove(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ClosedRoomWithBookingsEvenPastOnes_ReturnsConflictAndDoesNotRemove()
    {
        // HasAnyBookingsAsync counts every booking, past and cancelled included (rule 9).
        GivenRoom(RoomStatus.Closed);
        _rooms.Setup(r => r.HasAnyBookingsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.DeleteAsync(5, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.RoomHasBookings, result.ErrorCode);
        _rooms.Verify(r => r.Remove(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ClosedRoomWithoutBookings_RemovesAndSaves()
    {
        GivenRoom(RoomStatus.Closed);
        _rooms.Setup(r => r.HasAnyBookingsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _service.DeleteAsync(5, CancellationToken.None);

        Assert.Equal(ResultStatus.Success, result.Status);
        _rooms.Verify(r => r.Remove(It.Is<Room>(x => x.Id == 5)), Times.Once);
        _rooms.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateRoomNumber_ReturnsConflictAndDoesNotAdd()
    {
        _rooms.Setup(r => r.RoomNumberExistsAsync("101", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var request = new CreateRoomRequest
        {
            RoomNumber = "101", Name = "Duplicate", Floor = 1, Quality = QualityLevel.Standard,
            RoomType = RoomType.Regular, SeatCount = 25
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(ErrorCodes.RoomNumberTaken, result.ErrorCode);
        _rooms.Verify(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ComputerRoomWithoutComputerCount_ReturnsValidationError()
    {
        var request = new CreateRoomRequest
        {
            RoomNumber = "301", Name = "Lab", Floor = 3, Quality = QualityLevel.Premium,
            RoomType = RoomType.Computer, SeatCount = 20, ComputerCount = null
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(ResultStatus.ValidationError, result.Status);
        _rooms.Verify(r => r.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
