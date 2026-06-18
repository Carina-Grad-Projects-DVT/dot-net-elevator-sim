using ElevatorSim.Domain.Enums;

namespace ElevatorSim.Domain.ValueObjects;

/// <summary>
/// Represents a pickup request made from a floor (not from inside of elevator).
/// </summary>
public record PickupRequest
{
    /// <summary>
    /// Gets the floor where pickup is requested.
    /// </summary>
    public FloorNumber Floor { get; }

    /// <summary>
    /// Gets the requested direction of travel for pickup.
    /// If set to ElevatorDirection.None, the direction is unspecified.
    /// </summary>
    public ElevatorDirection Direction { get; }

    /// <summary>
    /// Gets the number of passengers currently awaiting pickup.
    /// </summary>
    public PassengerCount WaitingPassengers { get; }

    /// <summary>
    /// Gets the awaiting passenger count value.
    /// </summary>
    public int WaitingPassengerCount => WaitingPassengers.Value;

    /// <summary>
    /// Initializes a pickup request.
    /// </summary>
    /// <param name="floor">Requested pickup floor.</param>
    /// <param name="waitingPassengers">Number of passengers waiting at pickup floor. Number must be greater than zero.</param>
    /// <param name="direction">Requested travel direction. Use ElevatorDirection.None when unspecified.</param>
    public PickupRequest(
        FloorNumber floor,
        PassengerCount waitingPassengers,
        ElevatorDirection direction = ElevatorDirection.None
    )
    {
        if (waitingPassengers.Value < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(waitingPassengers),
                waitingPassengers.Value,
                "Waiting passengers must be at least 1."
            );
        }
        Floor = floor;
        WaitingPassengers = waitingPassengers;
        Direction = direction;
    }

    /// <summary>
    /// Creates a pickup request from values validated against floor range.
    /// </summary>
    /// <param name="floorValue">Requested floor value.</param>
    /// <param name="waitingPassengerCount">Count of waiting passengers.</param>
    /// <param name="direction">Requested direction of travel.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public static PickupRequest Create(
        int floorValue,
        int waitingPassengerCount,
        ElevatorDirection direction,
        int minimumFloor,
        int maximumFloor
    ) =>
        new(
            FloorNumber.Create(floorValue, minimumFloor, maximumFloor),
            new PassengerCount(waitingPassengerCount),
            direction
        );

    /// <summary>
    /// Creates a pickup request from values validated against floor range, with unspecified direction.
    /// </summary>
    /// <param name="floorValue">Requested floor value.</param>
    /// <param name="waitingPassengerCount">Count of waiting passengers.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public static PickupRequest Create(
        int floorValue,
        int waitingPassengerCount,
        int minimumFloor,
        int maximumFloor
    ) =>
        new(
            FloorNumber.Create(floorValue, minimumFloor, maximumFloor),
            new PassengerCount(waitingPassengerCount),
            ElevatorDirection.None
        );

    /// <summary>
    /// Creates a pickup request with a default waiting passenger count of 1.
    /// </summary>
    /// <param name="floorValue">Requested floor value.</param>
    /// <param name="direction">Requested travel direction.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public static PickupRequest Create(
        int floorValue,
        ElevatorDirection direction,
        int minimumFloor,
        int maximumFloor
    ) => Create(floorValue, 1, direction, minimumFloor, maximumFloor);

    /// <summary>
    /// Creates a pickup request with a default waiting passenger count of 1 and unspecified direction.
    /// </summary>
    /// <param name="floorValue">Requested floor value.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public static PickupRequest Create(int floorValue, int minimumFloor, int maximumFloor) =>
        Create(floorValue, 1, minimumFloor, maximumFloor);
}
