using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Entities;

/// <summary>
/// Represents a passenger elevator with state and capacity management.
/// </summary>
public sealed class Elevator
{
    /// <summary>
    /// Initializes a new Elevator instance.
    /// </summary>
    /// <param name="id">Unique elevator identifier.</param>
    /// <param name="startingFloor">Floor where the elevator initially starts.</param>
    /// <param name="maximumCapacity">Maximum number of passengers elevator can take.</param>
    public Elevator(ElevatorId id, FloorNumber startingFloor, int maximumCapacity)
    {
        if (maximumCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCapacity),
                maximumCapacity,
                "Maximum capacity must be 1 or more"
            );
        }

        Id = id;
        CurrentFloor = startingFloor;
        MaximumCapacity = maximumCapacity;
        CurrentPassengers = PassengerCount.Zero;
        Direction = Direction.None;
        MotionState = MotionState.Stationary;
        DoorState = DoorState.Closed;
    }

    /// <summary>
    /// Gets the elevator identifier.
    /// </summary>
    public ElevatorId Id { get; }

    /// <summary>
    /// Gets the elevator's current floor.
    /// </summary>
    public FloorNumber CurrentFloor { get; private set; }

    /// <summary>
    /// Gets the elevator's movement direction.
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    /// Gets whether the elevator is moving or stationary.
    /// </summary>
    public MotionState MotionState { get; private set; }

    /// <summary>
    /// Gets the elevator door state.
    /// </summary>
    public DoorState DoorState { get; private set; }

    /// <summary>
    /// Gets the maximum number of passengers this elevator can take.
    /// </summary>
    public int MaximumCapacity { get; }

    /// <summary>
    /// Gets the current number of passengers onboard.
    /// </summary>
    public PassengerCount CurrentPassengers { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the elevator is at capacity.
    /// </summary>
    public bool IsAtCapacity => CurrentPassengers.Value >= MaximumCapacity;

    /// <summary>
    /// Gets a value indicating whether the elevator is currently stationary.
    /// </summary>
    public bool IsStationary =>
        MotionState == MotionState.Stationary && Direction == Direction.None;

    /// <summary>
    /// Determines whether additional passengers can board without exceeding capacity.
    /// </summary>
    /// <param name="awaitingPassengers">Awaiting passenger count.</param>
    public bool CanBoardMore(PassengerCount awaitingPassengers) =>
        CurrentPassengers.Value + awaitingPassengers.Value <= MaximumCapacity;

    /// <summary>
    /// Adds passengers to the elevator if capacity allows.
    /// </summary>
    /// <param name="awaitingPassengers">Passenger count to board.</param>
    public void Board(PassengerCount awaitingPassengers)
    {
        var attemptedLoad = CurrentPassengers.Value + awaitingPassengers.Value;

        if (attemptedLoad > MaximumCapacity)
        {
            throw new InvalidOperationException(
                $"Boarding would exceed maximum capacity of {MaximumCapacity}. Attempted load: {attemptedLoad}."
            );
        }

        CurrentPassengers = new PassengerCount(attemptedLoad);
    }

    /// <summary>
    /// Removes passengers from the elevator.
    /// </summary>
    /// <param name="disembarkingPassengers">Passenger count to disembark.</param>
    public void Disembark(PassengerCount disembarkingPassengers)
    {
        if (disembarkingPassengers.Value > CurrentPassengers.Value)
        {
            throw new InvalidOperationException(
                "Cannot disembark more passengers than are currently inside."
            );
        }

        CurrentPassengers = new PassengerCount(
            CurrentPassengers.Value - disembarkingPassengers.Value
        );
    }

    /// <summary>
    /// Updates the current floor.
    /// </summary>
    /// <param name="floor">New floor.</param>
    public void SetCurrentFloor(FloorNumber floor)
    {
        CurrentFloor = floor;
    }

    /// <summary>
    /// Updates movement state and direction.
    /// </summary>
    /// <param name="direction">Current travel direction.</param>
    /// <param name="motionState">Current motion state.</param>
    public void SetMovementState(Direction direction, MotionState motionState)
    {
        Direction = direction;
        MotionState = motionState;
    }

    /// <summary>
    /// Opens the elevator doors.
    /// </summary>
    public void OpenDoors()
    {
        if (MotionState == MotionState.Moving)
        {
            throw new InvalidOperationException(
                "Doors cannot be opened while the elevator is moving."
            );
        }

        DoorState = DoorState.Open;
    }

    /// <summary>
    /// Closes the elevator doors.
    /// </summary>
    public void CloseDoors()
    {
        DoorState = DoorState.Closed;
    }
}
