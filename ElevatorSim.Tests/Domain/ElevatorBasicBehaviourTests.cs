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
            maximumCapacity: 8
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
            maximumCapacity: 5
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
            maximumCapacity: 8
        );

        elevator.SetMovementState(Direction.Up, MotionState.Moving);

        var exception = Assert.Throws<InvalidOperationException>(() => elevator.OpenDoors());

        Assert.Contains("cannot be opened", exception.Message);
        Assert.Equal(DoorState.Closed, elevator.DoorState);
    }
}
