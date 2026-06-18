using System;
using System.Collections.Generic;
using System.Linq;
using ElevatorSimulation.Application;
using ElevatorSimulation.Domain.Entities;
using ElevatorSimulation.Domain.Enums;
using ElevatorSimulation.Domain.Models;

namespace ElevatorSimulation.Infrastructure.Presenters;

public class ConsoleUIPresenter : IElevatorPresenter
{
    private int _minimumFloor;
    private int _maximumFloor;

    public ConsoleUIPresenter(int minimumFloor, int maximumFloor)
    {
        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;
    }

    public void UpdateBuildingConfig(int minimumFloor, int maximumFloor)
    {
        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;
    }
}
