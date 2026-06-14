using ElevatorSim.Application.Controllers;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;
using ElevatorEntity = ElevatorSim.Domain.Entities.Elevator;

namespace ElevatorSim.Tests.Application;

public class ElevatorControllerTests
{
    [Fact]
    public void RequestStop_Returns_Success_And_Enqueues_Stop()
    {
        var controller = CreateController();

        var result = controller.RequestStop(3);
        var status = controller.GetStatus();

        Assert.True(result.Success);
        Assert.Equal(1, status.PendingStopCount);
    }

    [Fact]
    public void RequestStop_Returns_Failure_When_Floor_Is_Out_Of_Range()
    {
        var controller = CreateController();

        var result = controller.RequestStop(99);

        Assert.False(result.Success);
        Assert.Contains("between", result.Message);
    }

    [Fact]
    public void Board_Returns_Failure_When_Exceeding_Capacity()
    {
        var controller = CreateController(maximumCapacity: 5);
        controller.Board(4);

        var result = controller.Board(2);
        var status = controller.GetStatus();

        Assert.False(result.Success);
        Assert.Contains("exceed maximum capacity", result.Message);
        Assert.Equal(4, status.CurrentPassengers);
    }

    private static ElevatorController CreateController(int maximumCapacity = 8)
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: maximumCapacity,
            minimumFloor: -1,
            maximumFloor: 10
        );

        return new ElevatorController(elevator);
    }
}
