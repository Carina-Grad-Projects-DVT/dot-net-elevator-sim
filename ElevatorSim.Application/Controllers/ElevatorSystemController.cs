using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Controllers;

public class ElevatorSystemController
{
    // Using fleet to follow existing logistics theme
    private readonly List<IElevator> _elevatorFleet;
    private readonly IDispatchService _dispatchService;

    public IReadOnlyList<IElevator> ElevatorFleet => _elevatorFleet;

    public ElevatorSystemController(
        IEnumerable<IElevator> initialFleet,
        IDispatchService dispatchService
    )
    {
        ArgumentNullException.ThrowIfNull(initialFleet);
        _dispatchService =
            dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));

        _elevatorFleet = initialFleet.ToList();
        if (_elevatorFleet.Count == 0)
        {
            throw new ArgumentException(
                "Elevator fleet must contain at least one elevator.",
                nameof(initialFleet)
            );
        }
    }

    public IReadOnlyList<ElevatorStatus> GetFleetStatus() =>
        _elevatorFleet.Select(ElevatorStatus.From).ToList();

    public DispatchResult RequestPickup(PickupRequest request) =>
        _dispatchService.Dispatch(ElevatorFleet, request);

    public CommandResult StepAll()
    {
        try
        {
            foreach (var elevator in _elevatorFleet)
            {
                elevator.Step();
            }

            return CommandResult.Ok("Advanced all elevators by 1 tick.");
        }
        catch (Exception exception)
        {
            return CommandResult.Fail(exception.Message);
        }
    }

    public CommandResult AddElevator(IElevator elevator)
    {
        // Check if an existing elevator already has this id
        if (_elevatorFleet.Any(existingElevator => existingElevator.Id == elevator.Id))
        {
            return CommandResult.Fail($"Elevator {elevator.Id.Value} already exists in fleet.");
        }

        _elevatorFleet.Add(elevator);
        return CommandResult.Ok($"Elevator {elevator.Id.Value} added.");
    }
}
