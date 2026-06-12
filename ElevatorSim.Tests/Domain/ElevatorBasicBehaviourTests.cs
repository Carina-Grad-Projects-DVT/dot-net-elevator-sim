using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;
using Xunit;
using ElevatorEntity = ElevatorSim.Domain.Entities.Elevator;

namespace ElevatorSim.Tests.Domain.Elevator;

public class ElevatorBasicBehaviorTests
{
    [Fact]
    public void Boarding_Allows_Passengers_Within_Capacity()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: 8,
            minimumFloor: -1,
            maximumFloor: 10
        );

        elevator.Board(new PassengerCount(3));

        Assert.Equal(3, elevator.CurrentPassengers.Value);
        Assert.False(elevator.IsAtCapacity);
    }

    [Fact]
    public void Boarding_Throws_Invalid_Operation_Exception_When_Exceeding_Capacity()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: 5,
            minimumFloor: -1,
            maximumFloor: 10
        );

        elevator.Board(new PassengerCount(4));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            elevator.Board(new PassengerCount(2))
        );

        Assert.Contains("exceed maximum capacity", exception.Message);
        Assert.Equal(4, elevator.CurrentPassengers.Value);
    }

    [Fact]
    public void Open_Doors_While_Elevator_Is_Moving_Throws_Invalid_Operation_Exception()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: 8,
            minimumFloor: -1,
            maximumFloor: 10
        );

        elevator.SetMovementState(Direction.Up, MotionState.Moving);

        var exception = Assert.Throws<InvalidOperationException>(() => elevator.OpenDoors());

        Assert.Contains("cannot be opened", exception.Message);
        Assert.Equal(DoorState.Closed, elevator.DoorState);
    }

    [Fact]
    public void Close_Door_If_Open_Before_Moving()
    {
        // Arrange
        var elevator = new ElevatorEntity(
            id: new ElevatorId(2),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: 8,
            minimumFloor: -1,
            maximumFloor: 10
        );

        elevator.OpenDoors();
        elevator.RequestStop(2);

        // Act - tick 1 (should close doors only)
        elevator.Step();

        // Assert - no movement
        Assert.Equal(FloorNumber.Create(0, -1, 10), elevator.CurrentFloor);
        Assert.Equal(DoorState.Closed, elevator.DoorState);
        Assert.Equal(MotionState.Stationary, elevator.MotionState);
        Assert.Equal(Direction.None, elevator.Direction);

        // Act - tick 2 initialize movement
        elevator.Step();

        // Assert - moved one floor
        Assert.Equal(FloorNumber.Create(1, -1, 10), elevator.CurrentFloor);
        Assert.Equal(MotionState.Moving, elevator.MotionState);
        Assert.Equal(Direction.Up, elevator.Direction);
    }
}
