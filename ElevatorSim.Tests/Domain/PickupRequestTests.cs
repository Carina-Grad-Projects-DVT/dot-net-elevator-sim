using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Tests.Domain;

public class PickupRequestTests
{
    [Fact]
    public void Create_With_Valid_Inputs_Returns_PickupRequest()
    {
        var request = PickupRequest.Create(3, ElevatorDirection.Up, -1, 10);

        Assert.Equal(3, request.Floor.Value);
        Assert.Equal(ElevatorDirection.Up, request.Direction);
    }

    [Fact]
    public void Constructor_With_None_Direction_Throws()
    {
        var floor = FloorNumber.Create(0, -1, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PickupRequest(floor, ElevatorDirection.None)
        );
    }

    [Fact]
    public void Create_With_Out_Of_Range_Floor_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PickupRequest.Create(20, ElevatorDirection.Down, -1, 10)
        );
    }
}
