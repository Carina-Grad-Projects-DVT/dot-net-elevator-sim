namespace ElevatorSim.Domain.ValueObjects;

/// <summary>
/// Strongly typed value object for passenger counts.
/// </summary>
public readonly record struct PassengerCount
{
    public int Value { get; }

    public static PassengerCount Zero => new(0);

    private PassengerCount(int value)
    {
        Value = value;
    }

    public static PassengerCount Create(int value, int maximumPassengers)
    {
        if (maximumPassengers < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumPassengers),
                maximumPassengers,
                "Maximum passengers must be at least 1."
            );
        }

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Passenger count cannot be negative."
            );
        }

        if (value > maximumPassengers)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Passenger count cannot exceed maximum capacity ({maximumPassengers})."
            );
        }

        return new PassengerCount(value);
    }
}
