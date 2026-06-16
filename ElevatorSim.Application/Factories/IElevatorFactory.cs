using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Interfaces;

namespace ElevatorSim.Application.Factories;

/// <summary>
/// Creates elevator instances using configuration.
/// </summary>
public interface IElevatorFactory
{
    IElevator Create(ElevatorConfiguration configuration);
    IReadOnlyList<IElevator> CreateMany(IEnumerable<ElevatorConfiguration> configurations);
}
