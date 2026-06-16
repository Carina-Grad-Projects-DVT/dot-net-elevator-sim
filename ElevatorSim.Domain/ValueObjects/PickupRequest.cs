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
    /// If set to <see cref="ElevatorDirection.None"/>, the direction is unspecified.
    /// </summary>
    public ElevatorDirection Direction { get; }

    /// <summary>
    /// Initializes a pickup request.
    /// </summary>
    /// <param name="floor">Requested pickup floor.</param>
    /// <param name="direction">Requested travel direction. Use ElevatorDirection.None when unspecified.</param>
    public PickupRequest(FloorNumber floor, ElevatorDirection direction)
    {
        Floor = floor;
        Direction = direction;
    }

    /// <summary>
    /// Creates a pickup request from values validated against floor range.
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
    ) => new(FloorNumber.Create(floorValue, minimumFloor, maximumFloor), direction);

    /// <summary>
    /// Creates a pickup request from values validated against floor range, with unspecified direction.
    /// </summary>
    /// <param name="floorValue">Requested floor value.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public static PickupRequest Create(int floorValue, int minimumFloor, int maximumFloor) =>
        new(FloorNumber.Create(floorValue, minimumFloor, maximumFloor), ElevatorDirection.None);
}
