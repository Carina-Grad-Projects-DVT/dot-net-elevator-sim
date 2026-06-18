using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Interfaces;

public interface IFreightElevator : IElevator
{
    decimal CurrentLoadKg { get; }
    decimal MaximumLoadKg { get; }
    bool IsAtLoadCapacity { get; }

    bool CanLoad(decimal loadKg);
    void Load(decimal loadKg);
    void Unload(decimal loadKg);
}
