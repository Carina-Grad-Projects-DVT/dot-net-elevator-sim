using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Entities;

/// <summary>
/// Placeholder freight elevator implementation.
/// </summary>
public sealed class FreightElevator : ElevatorBase
{
    /// <summary>
    /// Initializes a new placeholder freight elevator.
    /// </summary>
    /// <param name="id">Unique elevator identifier.</param>
    /// <param name="startingFloor">Floor where the elevator initially starts.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public FreightElevator(
        ElevatorId id,
        FloorNumber startingFloor,
        int minimumFloor,
        int maximumFloor
    )
        : base(id, startingFloor, minimumFloor, maximumFloor) { }
}
