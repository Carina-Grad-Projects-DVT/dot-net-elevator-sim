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

        var eligibleElevators = elevators
            .OfType<IPassengerElevator>()
            .Where(elevator => !elevator.IsAtPassengerCapacity)
            .Cast<IElevator>()
            .ToList();

        if (eligibleElevators.Count == 0)
        {
            return DispatchResult.Rejected(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                // TODO: Better message
                reason: "No eligible passenger elevators are currently available."
            );
        }

        var immediatelyAvailableElevators = eligibleElevators
            .Where(IsAvailableImmediately)
            .ToList();

        var eligibleElevatorCandidates =
            immediatelyAvailableElevators.Count > 0
                ? immediatelyAvailableElevators
                : eligibleElevators;

        var selectedElevator = SelectNearestElevator(eligibleElevatorCandidates, request);
        var estimatedArrivalTicks = EstimateArrivalTicks(selectedElevator, request);

        selectedElevator.RequestStop(request.Floor);

        if (immediatelyAvailableElevators.Count > 0)
        {
            return DispatchResult.Assigned(
                requestFloor: request.Floor.Value,
                requestDirection: request.Direction,
                elevatorId: selectedElevator.Id.Value,
                estimatedArrivalTicks: estimatedArrivalTicks
            );
        }

        return DispatchResult.Queued(
            requestFloor: request.Floor.Value,
            requestDirection: request.Direction,
            elevatorId: selectedElevator.Id.Value,
            message: $"All eligible elevators are currently busy. Request queued for elevator {selectedElevator.Id.Value}."
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
