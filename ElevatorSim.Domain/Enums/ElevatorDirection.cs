namespace ElevatorSim.Domain.Enums;

/// <summary>
/// Represents an elevator's current direction of motion.
/// </summary>
public enum ElevatorDirection
{
    /// <summary>
    /// Elevator is at a stand still.
    /// </summary>
    None = 0,

    /// <summary>
    /// Elevator is moving up.
    /// </summary>
    Up = 1,

    /// <summary>
    /// Elevator is moving down.
    /// </summary>
    Down = -1,
}
