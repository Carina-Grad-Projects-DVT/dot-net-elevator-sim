using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Services;

public interface IDispatchService
{
    DispatchResult Dispatch(IReadOnlyCollection<IElevator> elevators, PickupRequest request);
}
