using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Tests.Domain;

public class PickupRequestTests
{
    [Fact]
    public void Create_With_Valid_Inputs_Returns_Pickup_Request()
    {
        var request = PickupRequest.Create(3, 4, ElevatorDirection.Up, -1, 10);

        Assert.Equal(3, request.Floor.Value);
        Assert.Equal(ElevatorDirection.Up, request.Direction);
        Assert.Equal(4, request.WaitingPassengerCount);
    }

    [Fact]
    public void Create_With_Out_Of_Range_Floor_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PickupRequest.Create(20, 2, ElevatorDirection.Down, -1, 10)
        );
    }
}
