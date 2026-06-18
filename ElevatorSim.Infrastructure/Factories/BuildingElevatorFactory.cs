using ElevatorSimulation.Application;
using ElevatorSimulation.Domain.Entities;

namespace ElevatorSimulation.Infrastructure.Factories;

public class BuildingElevatorFactory : IElevatorFactory
{
    private readonly IElevatorFactory _elevatorFactory;

    public BuildingElevatorFactory(IElevatorFactory elevatorFactory)
    {
        _elevatorFactory = elevatorFactory;
    }
}
