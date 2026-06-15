using ElevatorSim.Domain.Entities;
using ElevatorSim.Domain.Enums;

namespace ElevatorSim.Application.Models;

/// <summary>
/// Read-model snapshot of the elevator's current state.
/// </summary>
/// <param name="ElevatorId">Numeric identifier of the elevator.</param>
/// <param name="CurrentFloor">Raw integer floor value.</param>
/// <param name="CurrentFloorDisplay">Display floor label (example : B1/G/5).</param>
/// <param name="Direction">Current travel direction.</param>
/// <param name="MotionState">Current motion state.</param>
/// <param name="DoorState">Current door state.</param>
/// <param name="CurrentPassengers">Current passengers on board count.</param>
/// <param name="MaximumCapacity">Maximum passenger capacity.</param>
/// <param name="PendingStopCount">Number of queued stops.</param>
/// <param name="IsAtCapacity">Is elevator currently at capacity?</param>
/// <param name="IsStationary">Is elevator currently stationary?</param>
public sealed record ElevatorStatus(
    int ElevatorId,
    int CurrentFloor,
    string CurrentFloorDisplay,
    ElevatorDirection Direction,
    MotionState MotionState,
    DoorState DoorState,
    int CurrentPassengers,
    int MaximumCapacity,
    int PendingStopCount,
    bool IsAtCapacity,
    bool IsStationary
)
{
    public static ElevatorStatus From(Elevator elevator) =>
        new(
            ElevatorId: elevator.Id.Value,
            CurrentFloor: elevator.CurrentFloor.Value,
            CurrentFloorDisplay: elevator.CurrentFloor.ToString(),
            Direction: elevator.Direction,
            MotionState: elevator.MotionState,
            DoorState: elevator.DoorState,
            CurrentPassengers: elevator.CurrentPassengers.Value,
            MaximumCapacity: elevator.MaximumCapacity,
            PendingStopCount: elevator.PendingStopCount,
            IsAtCapacity: elevator.IsAtCapacity,
            IsStationary: elevator.IsStationary
        );
}
