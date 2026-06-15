using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Exceptions;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Entities;

/// <summary>
/// Provides shared elevator movement, stop queueing, and door-state behavior.
/// </summary>
public abstract class ElevatorBase : IElevator
{
    private readonly int _minimumFloor;
    private readonly int _maximumFloor;
    private readonly Queue<FloorNumber> _pendingStops = new();

    /// <summary>
    /// Initializes a new base elevator instance with a restricted floor range.
    /// </summary>
    /// <param name="id">Unique elevator identifier.</param>
    /// <param name="startingFloor">Floor where the elevator initially starts.</param>
    /// <param name="minimumFloor">Minimum supported floor for this elevator.</param>
    /// <param name="maximumFloor">Maximum supported floor for this elevator.</param>
    protected ElevatorBase(
        ElevatorId id,
        FloorNumber startingFloor,
        int minimumFloor,
        int maximumFloor
    )
    {
        if (minimumFloor > maximumFloor)
        {
            throw new InvalidFloorException(
                $"Minimum floor ({minimumFloor}) cannot be greater than maximum floor ({maximumFloor})."
            );
        }

        if (!FloorNumber.IsInRange(startingFloor.Value, minimumFloor, maximumFloor))
        {
            throw new InvalidFloorException(startingFloor.Value, minimumFloor, maximumFloor);
        }

        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;

        Id = id;
        CurrentFloor = startingFloor;
        Direction = ElevatorDirection.None;
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
    public FloorNumber CurrentFloor { get; protected set; }

    /// <summary>
    /// Gets the elevator's current direction of travel.
    /// </summary>
    public ElevatorDirection Direction { get; protected set; }

    /// <summary>
    /// Gets whether the elevator is moving or stationary.
    /// </summary>
    public MotionState MotionState { get; protected set; }

    /// <summary>
    /// Gets the elevator door state.
    /// </summary>
    public DoorState DoorState { get; protected set; }

    /// <summary>
    /// Gets the number of pending queued stops.
    /// </summary>
    public int PendingStopCount => _pendingStops.Count;

    /// <summary>
    /// Gets a value indicating whether the elevator is currently stationary.
    /// </summary>
    public bool IsStationary =>
        MotionState == MotionState.Stationary && Direction == ElevatorDirection.None;

    /// <summary>
    /// Queues a stop request by raw floor value.
    /// </summary>
    /// <param name="floorValue">Requested stop floor value.</param>
    public void RequestStop(int floorValue)
    {
        var requestedFloor = FloorNumber.Create(floorValue, _minimumFloor, _maximumFloor);
        RequestStop(requestedFloor);
    }

    /// <summary>
    /// Queues a stop request by floor number.
    /// </summary>
    /// <param name="requestedFloor">Requested stop floor.</param>
    public void RequestStop(FloorNumber requestedFloor)
    {
        if (!FloorNumber.IsInRange(requestedFloor.Value, _minimumFloor, _maximumFloor))
        {
            throw new InvalidFloorException(requestedFloor.Value, _minimumFloor, _maximumFloor);
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
    /// Advances the elevator simulation by one tick.
    /// </summary>
    public void Step()
    {
        if (_pendingStops.Count == 0)
        {
            MotionState = MotionState.Stationary;
            Direction = ElevatorDirection.None;
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

        Direction =
            targetFloor.Value > CurrentFloor.Value ? ElevatorDirection.Up : ElevatorDirection.Down;
        MotionState = MotionState.Moving;

        var floorChange = Direction == ElevatorDirection.Up ? 1 : -1;
        var nextFloorValue = CurrentFloor.Value + floorChange;
        CurrentFloor = FloorNumber.Create(nextFloorValue, _minimumFloor, _maximumFloor);

        if (CurrentFloor == targetFloor)
        {
            ArriveAtTargetFloor();
        }
    }

    /// <summary>
    /// Advances the elevator simulation by a number of ticks.
    /// </summary>
    /// <param name="tickCount">Number of ticks to process.</param>
    public void AdvanceTicks(int tickCount)
    {
        if (tickCount < 1)
        {
            throw new InvalidElevatorOperationException(
                $"Tick count must be 1 or more. Received {tickCount}."
            );
        }

        for (var i = 0; i < tickCount; i++)
        {
            Step();
        }
    }

    /// <summary>
    /// Opens the elevator doors.
    /// </summary>
    protected void OpenDoors()
    {
        if (MotionState == MotionState.Moving)
        {
            throw new InvalidElevatorOperationException(
                "Doors cannot be opened while the elevator is in motion."
            );
        }

        DoorState = DoorState.Open;
    }

    /// <summary>
    /// Closes the elevator doors.
    /// </summary>
    protected void CloseDoors()
    {
        DoorState = DoorState.Closed;
    }

    private void ArriveAtTargetFloor()
    {
        _pendingStops.Dequeue();
        MotionState = MotionState.Stationary;
        Direction = ElevatorDirection.None;
        OpenDoors();
        OnArrivedAtFloor();
    }

    /// <summary>
    /// Executes custom logic when the elevator arrives at a target floor.
    /// </summary>
    protected virtual void OnArrivedAtFloor() { }
}
