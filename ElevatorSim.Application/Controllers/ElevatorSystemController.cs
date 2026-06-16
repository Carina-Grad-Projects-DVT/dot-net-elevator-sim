using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Interfaces;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Controllers;

public class ElevatorSystemController
{
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
}
