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
public class ElevatorFactory : IElevatorFactory
{
    public IElevator Create(ElevatorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.ElevatorCreationValidation();
        return configuration switch
        {
            PassengerElevatorConfiguration passengerConfiguration => CreatePassengerElevator(
                passengerConfiguration
            ),
            FreightElevatorConfiguration => throw new NotSupportedException(
                "Freight elevator creation is not available yet."
            ),
            _ => throw new ArgumentException(
                "Unsupported elevator configuration type.",
                nameof(configuration)
            ),
        };
    }

    private static int _nextElevatorId;

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

    private static ElevatorId CreateNextElevatorId()
    {
        return new ElevatorId(++_nextElevatorId);
    }

    private static IElevator CreatePassengerElevator(PassengerElevatorConfiguration configuration)
    {
        return new PassengerElevator(
            id: CreateNextElevatorId(),
            startingFloor: FloorNumber.Create(
                configuration.StartingFloor,
                configuration.MinimumFloor,
                configuration.MaximumFloor
            ),
            minimumFloor: configuration.MinimumFloor,
            maximumFloor: configuration.MaximumFloor,
            maximumPassengerCapacity: configuration.MaximumPassengerCapacity
        );
    }
}
