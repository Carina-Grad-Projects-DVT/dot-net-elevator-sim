using ElevatorSim.Application.Factories;
using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Entities;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.Interfaces;

namespace ElevatorSim.Tests.Application;

public class ElevatorFactoryTests
{
    [Fact]
    public void Create_With_Valid_Passenger_Config_Returns_Passenger_Elevator()
    {
        var factory = new ElevatorFactory();
        var configuration = new ElevatorConfiguration
        {
            Type = ElevatorType.Passenger,
            MinimumFloor = -1,
            MaximumFloor = 10,
            StartingFloor = 0,
            MaximumPassengerCapacity = 8,
        };

        var elevator = factory.Create(configuration);

        var passengerElevator = Assert.IsType<PassengerElevator>(elevator);
        Assert.True(passengerElevator.Id.Value > 0);
        Assert.Equal(0, passengerElevator.CurrentFloor.Value);
        Assert.Equal(8, passengerElevator.MaximumPassengerCapacity);
    }

    [Fact]
    public void Create_With_Freight_Config_Throws_NotSupported()
    {
        IElevatorFactory factory = new ElevatorFactory();
        var configuration = new ElevatorConfiguration
        {
            Type = ElevatorType.Freight,
            MinimumFloor = -1,
            MaximumFloor = 10,
            StartingFloor = 0,
        };

        var exception = Assert.Throws<NotSupportedException>(() => factory.Create(configuration));
        Assert.Contains("Freight elevator creation is not implemented yet", exception.Message);
    }
}
