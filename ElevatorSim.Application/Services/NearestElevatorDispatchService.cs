using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Services;

public class NearestElevatorDispatchService : IDispatchService
{
    public DispatchResult Dispatch(IReadOnlyCollection<IElevator> elevators, PickupRequest request)
    {
        ArgumentNullException.ThrowIfNull(elevators);
        ArgumentNullException.ThrowIfNull(request);
        var passengerElevators = elevators.OfType<IPassengerElevator>().ToList();
        if (passengerElevators.Count == 0)
        {
            return DispatchResult.Rejected(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                reason: "No passenger elevators are available in the fleet."
            );
        }

        var fleetMaximumCapacity = passengerElevators.Max(elevator =>
            elevator.MaximumPassengerCapacity
        );

        if (request.WaitingPassengerCount > fleetMaximumCapacity)
        {
            return DispatchResult.Rejected(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                reason: $"No elevator can ever board {request.WaitingPassengerCount} waiting passenger(s); fleet max capacity is {fleetMaximumCapacity}."
            );
        }

        var currentlyEligibleElevators = passengerElevators
            .Where(elevator => elevator.CanBoard(request.WaitingPassengers))
            .Cast<IElevator>()
            .ToList();

        if (currentlyEligibleElevators.Count == 0)
        {
            return DispatchResult.Queued(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                message: $"All passenger elevators are at capacity for {request.WaitingPassengerCount} waiting passenger(s). Request queued."
            );
        }

        var immediatelyAvailableElevators = currentlyEligibleElevators
            .Where(IsAvailableImmediately)
            .ToList();

        if (immediatelyAvailableElevators.Count == 0)
        {
            return DispatchResult.Queued(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                message: "All elevators are currently busy. Request queued."
            );
        }

        var selectedElevator = SelectNearestElevator(immediatelyAvailableElevators, request);
        var estimatedArrivalTicks = EstimateArrivalTicks(selectedElevator, request);

        selectedElevator.RequestStop(request.Floor);

        return DispatchResult.Assigned(
            requestFloor: request.Floor.Value,
            requestDirection: request.Direction,
            elevatorId: selectedElevator.Id.Value,
            estimatedArrivalTicks: estimatedArrivalTicks
        );
    }

    private static bool IsAvailableImmediately(IElevator elevator) =>
        elevator.IsStationary && elevator.PendingStopCount == 0;

    private static IElevator SelectNearestElevator(
        IEnumerable<IElevator> elevators,
        PickupRequest request
    ) =>
        elevators
            .OrderBy(elevator => Math.Abs(elevator.CurrentFloor.Value - request.Floor.Value))
            .ThenBy(elevator => elevator.PendingStopCount)
            .ThenBy(elevator => elevator.Id.Value)
            .First();

    private static int EstimateArrivalTicks(IElevator elevator, PickupRequest request)
    {
        var floorDistance = Math.Abs(elevator.CurrentFloor.Value - request.Floor.Value);
        if (floorDistance == 0)
        {
            return 0;
        }
        // Time penalty for an open door that needs to be closed first
        var openDoorTickPenalty = elevator.DoorState == DoorState.Open ? 1 : 0;
        return floorDistance + openDoorTickPenalty;
    }
}
