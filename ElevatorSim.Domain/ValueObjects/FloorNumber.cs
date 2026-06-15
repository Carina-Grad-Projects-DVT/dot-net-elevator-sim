namespace ElevatorSim.Domain.ValueObjects;

/// <summary>
/// Strongly typed floor number validated against a configurable building range.
/// Reusable across all elevator categories, including passenger and freight elevators.
/// Supports basement floors including negative values (example: -1 = B).
/// </summary>
public readonly record struct FloorNumber : IComparable<FloorNumber>
{
    /// <summary>
    /// Gets the underlying floor value.
    /// </summary>
    public int Value { get; }

    private FloorNumber(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a floor number using the building's configured min/max floor range.
    /// </summary>
    /// <param name="value">Requested floor.</param>
    /// <param name="minimumFloor">Minimum allowed floor (can be negative for basements).</param>
    /// <param name="maximumFloor">Maximum allowed floor.</param>
    public static FloorNumber Create(int value, int minimumFloor, int maximumFloor)
    {
        if (minimumFloor > maximumFloor)
        {
            throw new ArgumentException("Minimum floor cannot be greater than maximum floor.");
        }

        if (value < minimumFloor || value > maximumFloor)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Floor must be between {minimumFloor} and {maximumFloor}."
            );
        }
        return new FloorNumber(value);
    }

    /// <summary>
    /// Checks whether a raw floor value is within a configured building range.
    /// </summary>
    public static bool IsInRange(int value, int minimumFloor, int maximumFloor) =>
        minimumFloor <= maximumFloor && value >= minimumFloor && value <= maximumFloor;

    public int CompareTo(FloorNumber other) => Value.CompareTo(other.Value);

    /// <summary>
    /// Formats display value: basement floors as B1/B2, ground floor as G, upper floors as numeric.
    /// </summary>
    public override string ToString() =>
        Value switch
        {
            < 0 => $"B{Math.Abs(Value)}",
            0 => "G",
            _ => Value.ToString(),
        };
}
