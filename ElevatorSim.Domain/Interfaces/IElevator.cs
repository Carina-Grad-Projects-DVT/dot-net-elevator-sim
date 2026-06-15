using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Interfaces;

public interface IElevator
{
    ElevatorId Id { get; }
    FloorNumber CurrentFloor { get; }
    Direction Direction { get; }
    MotionState MotionState { get; }
    DoorState DoorState { get; }
    int PendingStopCount { get; }
    bool IsStationary { get; }

    // Method overload to pass data to ui
    void RequestStop(FloorNumber requestedFloor);

    // Method overload for code that already created a floor object
    void RequestStop(int floorValue);
    void Step();
    void AdvanceTicks(int tickCount);
}
