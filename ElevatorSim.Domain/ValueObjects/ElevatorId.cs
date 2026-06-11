namespace ElevatorSim.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for an elevator.
/// </summary>
public readonly record struct ElevatorId
{
    /// <summary>
    /// Gets the underlying numeric identifier value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Initializes a new ElevatorId instance.
    /// </summary>
    /// <param name="value">Identifier value; must be > 0.</param>
    public ElevatorId(int value)
    {
        if (value < 1)
        {
            throw new System.ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Elevator ID must be greater than 0."
            );
        }

        Value = value;
    }

    /// <summary>
    /// Returns a display-friendly label for the elevator.
    /// </summary>
    // Will print elevatorId as "E3" for example
    public override string ToString() => $"E{Value}";
}
