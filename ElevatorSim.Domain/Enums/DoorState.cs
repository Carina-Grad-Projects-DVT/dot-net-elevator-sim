namespace ElevatorSim.Domain.Enums;

/// <summary>
/// Represents what state the elevator's doors are in.
/// </summary>
public enum DoorState
{
    /// <summary>
    /// Elevator doors are close.
    /// </summary>
    Closed = 0,

    /// <summary>
    /// Elevator doors are opening.
    /// </summary>
    Opening = 1,

    /// <summary>
    /// Elevator doors are open.
    /// </summary>
    Open = 2,

    /// <summary>
    /// Elevator doors are closing.
    /// </summary>
    Closing = 3,
}
