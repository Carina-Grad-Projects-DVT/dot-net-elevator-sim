using ElevatorSim.Domain.Enums;

namespace ElevatorSim.Application.Models;

/// <summary>
/// Configuration used by the factory to create an elevator instance.
/// </summary>
public sealed record ElevatorConfiguration
{
    public required ElevatorType Type { get; init; }
    public required int MinimumFloor { get; init; }
    public required int MaximumFloor { get; init; }
    public required int StartingFloor { get; init; }

    // For Passengers
    public int? MaximumPassengerCapacity { get; init; }

    // For Freight (Unused for now)
    public decimal? MaximumFreightLoadKg { get; init; }

    public void ElevatorCreationValidation()
    {
        if (MinimumFloor > MaximumFloor)
        {
            throw new ArgumentException("MinimumFloor cannot be greater than MaximumFloor.");
        }

        if (StartingFloor < MinimumFloor || StartingFloor > MaximumFloor)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StartingFloor),
                $"StartingFloor must be between {MinimumFloor} and {MaximumFloor}."
            );
        }

        if (Type == ElevatorType.Passenger)
        {
            if (MaximumPassengerCapacity is null || MaximumPassengerCapacity.Value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaximumPassengerCapacity),
                    "MaximumPassengerCapacity must be set and greater than 0 passengers"
                );
            }
        }

        if (Type == ElevatorType.Freight)
        {
            throw new NotSupportedException("Freight elevator creation is not implemented yet.");
        }
    }
}
