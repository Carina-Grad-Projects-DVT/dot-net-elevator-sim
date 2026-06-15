using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Entities;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Factories;

/// <summary>
/// Factory for elevator creation.
/// Only passenger elevators are implemented for now
/// </summary>
public sealed class ElevatorFactory : IElevatorFactory
{
    public IElevator Create(ElevatorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.ElevatorCreationValidation();

        return configuration.Type switch
        {
            ElevatorType.Passenger => CreatePassengerElevator(configuration),
            ElevatorType.Freight => throw new NotSupportedException(
                "Freight elevator creation is not available yet."
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(configuration.Type),
                configuration.Type,
                "Unsupported elevator type."
            ),
        };
    }

    public IReadOnlyList<IElevator> CreateMany(IEnumerable<ElevatorConfiguration> configurations)
    {
        ArgumentNullException.ThrowIfNull(configurations);

        var elevators = new List<IElevator>();
        foreach (var configuration in configurations)
        {
            elevators.Add(Create(configuration));
        }

        return elevators;
    }

    private static IElevator CreatePassengerElevator(ElevatorConfiguration configuration)
    {
        if (configuration.MaximumPassengerCapacity is null)
        {
            throw new ArgumentException(
                "MaximumPassengerCapacity is required",
                nameof(configuration)
            );
        }

        return new PassengerElevator(
            id: new ElevatorId(configuration.Id),
            startingFloor: FloorNumber.Create(
                configuration.StartingFloor,
                configuration.MinimumFloor,
                configuration.MaximumFloor
            ),
            minimumFloor: configuration.MinimumFloor,
            maximumFloor: configuration.MaximumFloor,
            maximumPassengerCapacity: configuration.MaximumPassengerCapacity.Value
        );
    }
}
