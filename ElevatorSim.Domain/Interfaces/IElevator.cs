public interface IElevator
{
    ElevatorId Id { get; }
    FloorNumber CurrentFloor { get; }
    int MaximumCapacity { get; }
    PassengerCount CurrentPassengers { get; }

    void RequestStop(int floorValue);
    void Board(PassengerCount passengers);
    void Disembark(PassengerCount passengers);
    void Step();
}
