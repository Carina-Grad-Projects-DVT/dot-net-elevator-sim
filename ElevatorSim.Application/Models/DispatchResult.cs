using ElevatorSim.Domain.Enums;

namespace ElevatorSim.Application.Models;

public enum DispatchOutcome
{
    Assigned = 0,
    Queued = 1,
    Rejected = 2,
}

public record DispatchResult(
    DispatchOutcome Outcome,
    string Message,
    int RequestFloor,
    ElevatorDirection RequestDirection,
    // Elevator that dispatch service assigned to the request
    int? AssignedElevatorId,
    // Example process : if elevator is on floor 2, request is floor 5, no interruptions, doors already closed -> about 3 ticks.
    int? EstimatedArrivalTicks
)
{
    public bool IsSuccess => Outcome is DispatchOutcome.Assigned or DispatchOutcome.Queued;

    public static DispatchResult Assigned(
        int requestFloor,
        ElevatorDirection requestDirection,
        int elevatorId,
        int estimatedArrivalTicks
    ) =>
        new(
            Outcome: DispatchOutcome.Assigned,
            Message: $"Elevator {elevatorId} dispatched to floor {requestFloor}.",
            RequestFloor: requestFloor,
            RequestDirection: requestDirection,
            AssignedElevatorId: elevatorId,
            EstimatedArrivalTicks: estimatedArrivalTicks
        );

    public static DispatchResult Queued(
        int requestFloor,
        ElevatorDirection requestDirection,
        int elevatorId,
        string message
    ) =>
        new(
            Outcome: DispatchOutcome.Queued,
            Message: message,
            RequestFloor: requestFloor,
            RequestDirection: requestDirection,
            AssignedElevatorId: elevatorId,
            EstimatedArrivalTicks: null
        );

    public static DispatchResult Rejected(
        int requestFloor,
        ElevatorDirection requestDirection,
        string reason
    ) =>
        new(
            Outcome: DispatchOutcome.Rejected,
            Message: reason,
            RequestFloor: requestFloor,
            RequestDirection: requestDirection,
            AssignedElevatorId: null,
            EstimatedArrivalTicks: null
        );
}
