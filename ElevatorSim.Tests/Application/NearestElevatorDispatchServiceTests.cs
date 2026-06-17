using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;
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
        var request = PickupRequest.Create(4, 2, ElevatorDirection.Up, -1, 10);

        var result = service.Dispatch(elevators, request);

        Assert.Equal(DispatchOutcome.Assigned, result.Outcome);
        Assert.Equal(3, result.AssignedElevatorId);
        Assert.Equal(1, result.EstimatedArrivalTicks);
    }

    [Fact]
    public void Dispatch_Returns_Queued_Without_Assignment_When_All_Eligible_Elevators_Are_Busy()
    {
        var service = new NearestElevatorDispatchService();
        var firstElevator = CreatePassengerElevator(id: 1, currentFloor: 1);
        var secondElevator = CreatePassengerElevator(id: 2, currentFloor: 2);

        firstElevator.RequestStop(4);
        secondElevator.RequestStop(5);

        var result = service.Dispatch(
            new IElevator[] { firstElevator, secondElevator },
            PickupRequest.Create(6, 2, ElevatorDirection.Up, -1, 10)
        );

        Assert.Equal(DispatchOutcome.Queued, result.Outcome);
        Assert.Null(result.AssignedElevatorId);
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
