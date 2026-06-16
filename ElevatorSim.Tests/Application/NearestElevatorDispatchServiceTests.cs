using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;
using FreightElevatorEntity = ElevatorSim.Domain.Entities.FreightElevator;
using PassengerElevatorEntity = ElevatorSim.Domain.Entities.PassengerElevator;

namespace ElevatorSim.Tests.Application;

public class NearestElevatorDispatchServiceTests
{
    [Fact]
    public void Dispatch_Selects_Nearest_Immediately_Available_Passenger_Elevator()
    {
        var service = new NearestElevatorDispatchService();
        var elevators = new IElevator[]
        {
            CreatePassengerElevator(id: 1, currentFloor: 0),
            CreatePassengerElevator(id: 2, currentFloor: 6),
            CreatePassengerElevator(id: 3, currentFloor: 3),
        };
        var request = PickupRequest.Create(4, ElevatorDirection.Up, -1, 10);

        var result = service.Dispatch(elevators, request);

        Assert.Equal(DispatchOutcome.Assigned, result.Outcome);
        Assert.Equal(3, result.AssignedElevatorId);
        Assert.Equal(1, result.EstimatedArrivalTicks);
    }

    [Fact]
    public void Dispatch_Skips_Nearest_Elevator_When_At_Capacity()
    {
        var service = new NearestElevatorDispatchService();
        var fullElevator = CreatePassengerElevator(id: 1, currentFloor: 4, maximumCapacity: 1);
        var nextElevator = CreatePassengerElevator(id: 2, currentFloor: 7);
        fullElevator.Board(new PassengerCount(1));
        var request = PickupRequest.Create(5, ElevatorDirection.Up, -1, 10);

        var result = service.Dispatch(new IElevator[] { fullElevator, nextElevator }, request);

        Assert.Equal(DispatchOutcome.Assigned, result.Outcome);
        Assert.Equal(2, result.AssignedElevatorId);
    }

    private static IPassengerElevator CreatePassengerElevator(
        int id,
        int currentFloor,
        int maximumCapacity = 8,
        int minimumFloor = -1,
        int maximumFloor = 10
    )
    {
        return new PassengerElevatorEntity(
            new ElevatorId(id),
            FloorNumber.Create(currentFloor, minimumFloor, maximumFloor),
            minimumFloor,
            maximumFloor,
            maximumCapacity
        );
    }
}
