using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;
using Xunit;
using ElevatorEntity = ElevatorSim.Domain.Entities.PassengerElevator;

namespace ElevatorSim.Tests.Domain.Elevator;

public class ElevatorBasicBehaviorTests
{
    [Fact]
    public void Boarding_Allows_Passengers_Within_Capacity()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            minimumFloor: -1,
            maximumFloor: 10,
            maximumPassengerCapacity: 8
        );

        elevator.Board(new PassengerCount(3));

        Assert.Equal(3, elevator.CurrentPassengers.Value);
        Assert.False(elevator.IsAtPassengerCapacity);
    }

    [Fact]
    public void Boarding_Throws_Invalid_Operation_Exception_When_Exceeding_Capacity()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            minimumFloor: -1,
            maximumFloor: 10,
            maximumPassengerCapacity: 5
        );

        elevator.Board(new PassengerCount(4));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            elevator.Board(new PassengerCount(2))
        );

        Assert.Contains("Passenger capacity exceeded", exception.Message);
        Assert.Equal(4, elevator.CurrentPassengers.Value);
    }

    [Fact]
    public void Requesting_Stop_At_Current_Floor_Opens_Doors_When_Stationary()
    {
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            minimumFloor: -1,
            maximumFloor: 10,
            maximumPassengerCapacity: 8
        );

        elevator.RequestStop(0);

        Assert.Equal(DoorState.Open, elevator.DoorState);
        Assert.Equal(MotionState.Stationary, elevator.MotionState);
        Assert.Equal(ElevatorDirection.None, elevator.Direction);
    }

    [Fact]
    public void Close_Door_If_Open_Before_Moving()
    {
        // Arrange
        var elevator = new ElevatorEntity(
            id: new ElevatorId(2),
            startingFloor: FloorNumber.Create(0, -1, 10),
            minimumFloor: -1,
            maximumFloor: 10,
            maximumPassengerCapacity: 8
        );

        elevator.RequestStop(0);
        elevator.RequestStop(2);

        // Act - tick 1 (should close doors only)
        elevator.Step();

        // Assert - no movement
        Assert.Equal(FloorNumber.Create(0, -1, 10), elevator.CurrentFloor);
        Assert.Equal(DoorState.Closed, elevator.DoorState);
        Assert.Equal(MotionState.Stationary, elevator.MotionState);
        Assert.Equal(ElevatorDirection.None, elevator.Direction);

        // Act - tick 2 initialize movement
        elevator.Step();

        // Assert - moved one floor
        Assert.Equal(FloorNumber.Create(1, -1, 10), elevator.CurrentFloor);
        Assert.Equal(MotionState.Moving, elevator.MotionState);
        Assert.Equal(ElevatorDirection.Up, elevator.Direction);
    }
}
