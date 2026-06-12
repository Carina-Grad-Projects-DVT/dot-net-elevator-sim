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
        // Arrange
        var elevator = new ElevatorEntity(
            id: new ElevatorId(1),
            startingFloor: FloorNumber.Create(0, -1, 10),
            maximumCapacity: 8
        );

        // Act
        elevator.Board(new PassengerCount(3));

        // Assert
        Assert.Equal(3, elevator.CurrentPassengers.Value);
        Assert.False(elevator.IsAtCapacity);
    }
}
