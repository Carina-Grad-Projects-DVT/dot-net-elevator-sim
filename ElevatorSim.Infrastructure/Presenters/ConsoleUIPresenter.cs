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

    private void UI()
    // First ask for building size (how many floors) + elevator amount with their types (only passenger elevator for now)
    // After getting this info create something like this :
    {
        Console.WriteLine(
            "========================================================================="
        );
        Console.WriteLine(
            "                           ELEVATOR SIMULATION                           "
        );
        Console.WriteLine(
            "========================================================================="
        );
        Console.WriteLine("Elevators here");
        // Example with 3 elevators
        // Floor 7  |               |               | [E3:▼(1/4)]   | Queue:
        // Floor 6  |               |               |               | Queue:
        // Floor 5  |               | [E2:■(0/4)]   |               | Queue: ● (1 waiting)
        // Floor 4  |               |               |               | Queue:
        // Floor 3  | [E1:▲(3/4)]   |               |               | Queue:
        // Floor 2  |               |               |               | Queue:
        // Floor 1  |               |               |               | Queue:
        // Floor G  |               |               |               | Queue: ●●● (3 waiting)
        // Floor B1 |               |               |               | Queue:
        // -----------
        // Keys : ▲ Up, ▼ Down, ■ Stationary, ● Person in queue
        Console.WriteLine(
            "========================================================================="
        );
        Console.WriteLine("Status goes here");
        Console.WriteLine(
            "========================================================================="
        );
        Console.WriteLine("Instructions goes here");
    }
}
