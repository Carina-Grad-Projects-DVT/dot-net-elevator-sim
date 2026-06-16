namespace ElevatorSim.Domain.Exceptions;

/// <summary>
/// Exception thrown when a floor value is outside the configured range.
/// </summary>
public class InvalidFloorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidFloorException class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public InvalidFloorException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the InvalidFloorException class.
    /// </summary>
    /// <param name="floor">Invalid floor value.</param>
    /// <param name="minimumFloor">Minimum supported floor.</param>
    /// <param name="maximumFloor">Maximum supported floor.</param>
    public InvalidFloorException(int floor, int minimumFloor, int maximumFloor)
        : base($"Floor {floor} is outside the supported range of {minimumFloor} to {maximumFloor}.")
    { }
}
