namespace ElevatorSim.Domain.Enums;

/// <summary>
/// Represents whether an elevator is moving or not.
/// </summary>
public enum MotionState
{
    /// <summary>
    /// Elevator is not currently moving.
    /// </summary>
    Stationary = 0,

    /// <summary>
    /// Elevator is moving between floors.
    /// </summary>
    Moving = 1,
}
