namespace ElevatorSim.Application.Models;

/// <summary>
/// Base configuration for elevator creation.
/// </summary>
public record ElevatorConfiguration
{
    public required int MinimumFloor { get; init; }
    public required int MaximumFloor { get; init; }
    public required int StartingFloor { get; init; }

    public virtual void ElevatorCreationValidation()
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
    }
}

/// <summary>
/// Configuration specific to passenger elevators.
/// </summary>
public record PassengerElevatorConfiguration : ElevatorConfiguration
{
    public int MaximumPassengerCapacity { get; init; }

    public override void ElevatorCreationValidation()
    {
        base.ElevatorCreationValidation();

        if (MaximumPassengerCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumPassengerCapacity),
                "MaximumPassengerCapacity must be greater than 0 passengers."
            );
        }
    }
}

/// <summary>
/// Configuration specific to freight elevators.
/// </summary>
public record FreightElevatorConfiguration : ElevatorConfiguration
{
    public decimal MaximumFreightLoadKg { get; init; }

    public override void ElevatorCreationValidation()
    {
        base.ElevatorCreationValidation();

        if (MaximumFreightLoadKg <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumFreightLoadKg),
                "MaximumFreightLoadKg must be greater than 0."
            );
        }
    }
}
