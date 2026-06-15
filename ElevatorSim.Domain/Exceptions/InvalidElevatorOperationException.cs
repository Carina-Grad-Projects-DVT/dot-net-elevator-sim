namespace ElevatorSim.Domain.Exceptions;

/// <summary>
/// Exception thrown when an elevator operation is invalid for the elevator's current state.
/// </summary>
public sealed class InvalidElevatorOperationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidElevatorOperationException class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public InvalidElevatorOperationException(string message)
        : base(message) { }
}
