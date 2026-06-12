using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Entities;

/// <summary>
/// Represents a passenger elevator with state, stop queue, and tick-based movement.
/// </summary>
public sealed class Elevator
{
    private readonly int _minimumFloor;
    private readonly int _maximumFloor;
    private readonly Queue<FloorNumber> _pendingStops = new();

    /// <summary>
    /// Initializes a new Elevator instance with restricted floor range.
    /// </summary>
    /// <param name="id">Unique elevator identifier.</param>
    /// <param name="startingFloor">Floor where the elevator initially starts.</param>
    /// <param name="maximumCapacity">Maximum number of passengers the elevator can carry.</param>
    /// <param name="minimumFloor">Minimum supported floor for this elevator.</param>
    /// <param name="maximumFloor">Maximum supported floor for this elevator.</param>
    public Elevator(
        ElevatorId id,
        FloorNumber startingFloor,
        int maximumCapacity,
        int minimumFloor,
        int maximumFloor
    )
    {
        if (maximumCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCapacity),
                maximumCapacity,
                "Maximum capacity must be 1 or more."
            );
        }

        if (minimumFloor > maximumFloor)
        {
            throw new ArgumentException("Minimum floor cannot be greater than maximum floor.");
        }

        if (!FloorNumber.IsInRange(startingFloor.Value, minimumFloor, maximumFloor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startingFloor),
                $"Starting floor must be between {minimumFloor} and {maximumFloor}."
            );
        }

        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;

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
    /// Gets the maximum number of passengers this elevator can carry.
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
    /// Gets a value indicating whether at least one stop is queued.
    /// </summary>
    public bool HasPendingStops => _pendingStops.Count > 0;

    /// <summary>
    /// Gets the number of pending queued stops.
    /// </summary>
    public int PendingStopCount => _pendingStops.Count;

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
    /// Queues a stop request by floor value.
    /// </summary>
    /// <param name="floorValue">Requested stop floor.</param>
    public void RequestStop(int floorValue)
    {
        var requestedFloor = FloorNumber.Create(floorValue, _minimumFloor, _maximumFloor);
        RequestStop(requestedFloor);
    }

    /// <summary>
    /// Queues a stop request.
    /// </summary>
    /// <param name="requestedFloor">Requested stop floor.</param>
    public void RequestStop(FloorNumber requestedFloor)
    {
        if (!FloorNumber.IsInRange(requestedFloor.Value, _minimumFloor, _maximumFloor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedFloor),
                $"Requested floor must be between {_minimumFloor} and {_maximumFloor}."
            );
        }

        if (requestedFloor == CurrentFloor && IsStationary)
        {
            OpenDoors();
            return;
        }

        if (_pendingStops.Contains(requestedFloor))
        {
            return;
        }

        _pendingStops.Enqueue(requestedFloor);
    }

    /// <summary>
    /// Advances simulation by one tick.
    /// Each tick either closes doors, moves one floor toward target, or handles arrival state.
    /// </summary>
    public void Step()
    {
        if (_pendingStops.Count == 0)
        {
            MotionState = MotionState.Stationary;
            Direction = Direction.None;
            return;
        }

        var targetFloor = _pendingStops.Peek();

        if (CurrentFloor == targetFloor)
        {
            ArriveAtTargetFloor();
            return;
        }

        if (DoorState != DoorState.Closed)
        {
            CloseDoors();
            return;
        }

        Direction = targetFloor.Value > CurrentFloor.Value ? Direction.Up : Direction.Down;
        MotionState = MotionState.Moving;

        var floorChange = Direction == Direction.Up ? 1 : -1;
        var nextFloorValue = CurrentFloor.Value + floorChange;

        CurrentFloor = FloorNumber.Create(nextFloorValue, _minimumFloor, _maximumFloor);

        if (CurrentFloor == targetFloor)
        {
            ArriveAtTargetFloor();
        }
    }

    /// <summary>
    /// Advances simulation by a number of ticks.
    /// </summary>
    /// <param name="tickCount">Number of ticks to process.</param>
    // Advance reffering to "advance by 1"
    public void AdvanceTicks(int tickCount)
    {
        if (tickCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tickCount),
                tickCount,
                "Tick count must be 1 or more."
            );
        }

        for (var i = 0; i < tickCount; i++)
        {
            Step();
        }
    }

    /// <summary>
    /// Updates the current floor directly.
    /// </summary>
    /// <param name="floor">New floor.</param>
    public void SetCurrentFloor(FloorNumber floor)
    {
        if (!FloorNumber.IsInRange(floor.Value, _minimumFloor, _maximumFloor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(floor),
                $"Floor must be between {_minimumFloor} and {_maximumFloor}."
            );
        }

        CurrentFloor = floor;
    }

    /// <summary>
    /// Updates movement state and direction directly.
    /// </summary>
    /// <param name="direction">Current travel direction.</param>
    /// <param name="motionState">Current motion state.</param>
    public void SetMovementState(Direction direction, MotionState motionState)
    {
        if (motionState == MotionState.Moving && DoorState != DoorState.Closed)
        {
            throw new InvalidOperationException(
                "Doors must be closed before setting elevator to moving state."
            );
        }

        Direction = direction;
        MotionState = motionState;
    }

    /// <summary>
    /// Opens elevator doors.
    /// </summary>
    public void OpenDoors()
    {
        if (MotionState == MotionState.Moving)
        {
            throw new InvalidOperationException(
                "Doors cannot be opened while the elevator is in motion."
            );
        }

        DoorState = DoorState.Open;
    }

    /// <summary>
    /// Closes elevator doors.
    /// </summary>
    public void CloseDoors()
    {
        DoorState = DoorState.Closed;
    }

    private void ArriveAtTargetFloor()
    {
        _pendingStops.Dequeue();
        MotionState = MotionState.Stationary;
        Direction = Direction.None;
        OpenDoors();
    }
}
