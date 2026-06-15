using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;

namespace ElevatorSim.Application.Models;

/// <summary>
/// Read-model snapshot of the elevator's current state.
/// </summary>
/// <param name="ElevatorId">Numeric identifier of the elevator.</param>
/// <param name="ElevatorType">Elevator category for this status snapshot.</param>
/// <param name="CurrentFloor">Raw integer floor value.</param>
/// <param name="CurrentFloorDisplay">Display floor label (example : B1/G/5).</param>
/// <param name="Direction">Current travel direction.</param>
/// <param name="MotionState">Current motion state.</param>
/// <param name="DoorState">Current door state.</param>
/// <param name="CurrentPassengers">Current passengers on board count for passenger elevators.</param>
/// <param name="MaximumCapacity">Maximum passenger capacity for passenger elevators.</param>
/// <param name="PendingStopCount">Number of queued stops.</param>
/// <param name="IsAtCapacity">Whether a passenger elevator is currently at capacity.</param>
/// <param name="CurrentLoadKg">Current freight load in kilograms for freight elevators.</param>
/// <param name="MaximumLoadKg">Maximum freight load in kilograms for freight elevators.</param>
/// <param name="IsAtLoadCapacity">Whether a freight elevator is currently at load capacity.</param>
/// <param name="IsStationary">Is elevator currently stationary?</param>
public sealed record ElevatorStatus(
    int ElevatorId,
    ElevatorType ElevatorType,
    int CurrentFloor,
    string CurrentFloorDisplay,
    ElevatorDirection Direction,
    MotionState MotionState,
    DoorState DoorState,
    int? CurrentPassengers,
    int? MaximumCapacity,
    int PendingStopCount,
    bool? IsAtCapacity,
    decimal? CurrentLoadKg,
    decimal? MaximumLoadKg,
    bool? IsAtLoadCapacity,
    bool IsStationary
)
{
    /// <summary>
    /// Creates a status snapshot from any elevator implementation.
    /// </summary>
    /// <param name="elevator">Elevator instance to snapshot.</param>
    public static ElevatorStatus From(IElevator elevator)
    {
        ArgumentNullException.ThrowIfNull(elevator);

        var passengerElevator = elevator as IPassengerElevator;
        var freightElevator = elevator as IFreightElevator;

        var elevatorType = elevator switch
        {
            IFreightElevator => ElevatorType.Freight,
            IPassengerElevator => ElevatorType.Passenger,
            _ => ElevatorType.Passenger,
        };

        return new ElevatorStatus(
            ElevatorId: elevator.Id.Value,
            ElevatorType: elevatorType,
            CurrentFloor: elevator.CurrentFloor.Value,
            CurrentFloorDisplay: elevator.CurrentFloor.ToString(),
            Direction: elevator.Direction,
            MotionState: elevator.MotionState,
            DoorState: elevator.DoorState,
            CurrentPassengers: passengerElevator?.CurrentPassengers.Value,
            MaximumCapacity: passengerElevator?.MaximumPassengerCapacity,
            PendingStopCount: elevator.PendingStopCount,
            IsAtCapacity: passengerElevator?.IsAtPassengerCapacity,
            CurrentLoadKg: freightElevator?.CurrentLoadKg,
            MaximumLoadKg: freightElevator?.MaximumLoadKg,
            IsAtLoadCapacity: freightElevator?.IsAtLoadCapacity,
            IsStationary: elevator.IsStationary
        );
    }
}
