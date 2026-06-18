namespace ElevatorSim.Domain.ValueObjects;

/// <summary>
/// Strongly typed value object for passenger counts.
/// </summary>
public readonly record struct PassengerCount
{
    /// <summary>
    /// Gets the passenger count value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Represents zero (0) passengers.
    /// </summary>
    public static PassengerCount Zero => new(0);

    /// <summary>
    /// Initializes a new PassengerCount instance.
    /// </summary>
    /// <param name="value">Passenger count. Must be zero or greater.</param>
    public PassengerCount(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Passenger count cannot be negative."
            );
        }
        Value = value;
    }
}
