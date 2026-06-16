using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Interfaces;

public interface IPassengerElevator : IElevator
{
    PassengerCount CurrentPassengers { get; }
    int MaximumPassengerCapacity { get; }
    bool IsAtPassengerCapacity { get; }
    bool CanBoard(PassengerCount passengers);
    void Board(PassengerCount passengers);
    void Disembark(PassengerCount passengers);
}
