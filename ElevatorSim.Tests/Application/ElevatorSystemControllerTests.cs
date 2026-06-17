using ElevatorSim.Application.Controllers;
using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;
using ElevatorEntity = ElevatorSim.Domain.Entities.PassengerElevator;

namespace ElevatorSim.Tests.Application;

public class ElevatorSystemControllerTests
{
    [Fact]
    public void Request_Pickup_Queues_When_All_Eligible_Elevators_Are_Busy()
    {
        var elevator = CreatePassengerElevator(id: 1, startingFloor: 0);
        elevator.RequestStop(2);

        var controller = new ElevatorSystemController(
            new IElevator[] { elevator },
            new NearestElevatorDispatchService()
        );

        var request = PickupRequest.Create(3, 2, ElevatorDirection.Up, -1, 10);

        var result = controller.RequestPickup(request);

        Assert.Equal(DispatchOutcome.Queued, result.Outcome);
        Assert.Null(result.AssignedElevatorId);
        Assert.Equal(1, elevator.PendingStopCount);
        Assert.Equal(1, controller.QueuedPickupRequestCount);
    }

    private static IElevator CreatePassengerElevator(
        int id,
        int startingFloor,
        int minimumFloor = -1,
        int maximumFloor = 10,
        int maximumCapacity = 8
    )
    {
        return new ElevatorEntity(
            new ElevatorId(id),
            FloorNumber.Create(startingFloor, minimumFloor, maximumFloor),
            minimumFloor,
            maximumFloor,
            maximumCapacity
        );
    }
}
