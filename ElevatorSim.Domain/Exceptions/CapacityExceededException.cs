namespace ElevatorSim.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation would exceed a capacity limit as configured.
/// </summary>
public class CapacityExceededException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CapacityExceededException class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public CapacityExceededException(string message)
        : base(message) { }
}
