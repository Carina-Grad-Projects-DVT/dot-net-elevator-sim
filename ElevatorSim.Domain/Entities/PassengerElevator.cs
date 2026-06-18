using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Domain.Entities;

public class PassengerElevator : ElevatorBase, IPassengerElevator
{
    public int MaximumPassengerCapacity { get; }
    public PassengerCount CurrentPassengers { get; private set; } = PassengerCount.Zero;
    public bool IsAtPassengerCapacity => CurrentPassengers.Value >= MaximumPassengerCapacity;

    public PassengerElevator(
        ElevatorId id,
        FloorNumber startingFloor,
        int minimumFloor,
        int maximumFloor,
        int maximumPassengerCapacity
    )
        : base(id, startingFloor, minimumFloor, maximumFloor)
    {
        if (maximumPassengerCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPassengerCapacity));
        }

        MaximumPassengerCapacity = maximumPassengerCapacity;
    }

    public bool CanBoard(PassengerCount passengers) =>
        CurrentPassengers.Value + passengers.Value <= MaximumPassengerCapacity;

    public void Board(PassengerCount passengers)
    {
        if (!CanBoard(passengers))
        {
            throw new InvalidOperationException("Passenger capacity exceeded.");
        }

        CurrentPassengers = new PassengerCount(CurrentPassengers.Value + passengers.Value);
    }

    public void Disembark(PassengerCount passengers)
    {
        if (passengers.Value > CurrentPassengers.Value)
        {
            throw new InvalidOperationException("Cannot disembark more passengers than onboard.");
        }

        CurrentPassengers = new PassengerCount(CurrentPassengers.Value - passengers.Value);
    }
}
