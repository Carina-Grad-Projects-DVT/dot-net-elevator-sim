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
    private readonly Queue<PickupRequest> _waitingPickupRequests = new();

    public IReadOnlyList<IElevator> ElevatorFleet => _elevatorFleet;

    public int QueuedPickupRequestCount => _waitingPickupRequests.Count;

    public IReadOnlyList<PickupRequest> GetQueuedPickupRequestsSnapshot() =>
        _waitingPickupRequests.ToList();

    public IReadOnlyDictionary<int, int> GetQueuedWaitingCountsByFloor() =>
        _waitingPickupRequests
            .GroupBy(request => request.Floor.Value)
            .OrderBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(request => request.WaitingPassengerCount)
            );

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

    public DispatchResult RequestPickup(PickupRequest request)
    // Definition for merge :
    // Two requests are considered the same identity if they have the same floor & direction
    {
        ArgumentNullException.ThrowIfNull(request);

        if (TryMergeQueuedRequest(request, out var mergedQueuedRequest))
        {
            return DispatchResult.Queued(
                requestFloor: mergedQueuedRequest.Floor.Value,
                requestDirection: mergedQueuedRequest.Direction,
                message: $"Merged with existing queued request at floor {mergedQueuedRequest.Floor.Value}. Waiting passengers now {mergedQueuedRequest.WaitingPassengerCount}."
            );
        }
        var result = _dispatchService.Dispatch(ElevatorFleet, request);
        if (result.Outcome == DispatchOutcome.Queued)
        {
            _waitingPickupRequests.Enqueue(request);
        }

        return result;
    }

    public CommandResult StepAll()
    {
        try
        {
            foreach (var elevator in _elevatorFleet)
            {
                elevator.Step();
            }

            TryDispatchQueuedRequests();
            return CommandResult.Ok("Advanced all elevators by 1 tick.");
        }
        catch (InvalidFloorException ex)
        {
            return CommandResult.Fail(ex.Message);
        }
        catch (InvalidElevatorOperationException ex)
        {
            return CommandResult.Fail(ex.Message);
        }
        catch (CapacityExceededException ex)
        {
            return CommandResult.Fail(ex.Message);
        }
    }

    private void TryDispatchQueuedRequests()
    {
        if (_waitingPickupRequests.Count == 0)
        {
            return;
        }

        var remainingRequests = new Queue<PickupRequest>();
        while (_waitingPickupRequests.Count > 0)
        {
            var request = _waitingPickupRequests.Dequeue();
            var result = _dispatchService.Dispatch(ElevatorFleet, request);
            if (result.Outcome == DispatchOutcome.Queued)
            {
                remainingRequests.Enqueue(request);
            }
        }

        while (remainingRequests.Count > 0)
        {
            _waitingPickupRequests.Enqueue(remainingRequests.Dequeue());
        }
    }

    //  Check if the incoming pickup request matches any requests already in the queue
    private bool TryMergeQueuedRequest(PickupRequest request, out PickupRequest mergedRequest)
    {
        // Ensures param is always valid even if a merge doesn't happen
        mergedRequest = request;

        if (_waitingPickupRequests.Count == 0)
        {
            return false;
        }

        var queuedRequests = _waitingPickupRequests.ToList();

        var matchingEntries = queuedRequests
            .Select((queuedRequest, index) => new { queuedRequest, index })
            .Where(entry => IsSamePickupIdentity(entry.queuedRequest, request))
            .ToList();

        if (matchingEntries.Count == 0)
        {
            return false;
        }

        var mergedWaitingPassengerCount = request.WaitingPassengerCount;
        foreach (var entry in matchingEntries)
        {
            mergedWaitingPassengerCount = checked(
                mergedWaitingPassengerCount + entry.queuedRequest.WaitingPassengerCount
            );
        }

        mergedRequest = new PickupRequest(
            request.Floor,
            new PassengerCount(mergedWaitingPassengerCount),
            request.Direction
        );

        var firstMatchingIndex = matchingEntries[0].index;
        var deduplicatedQueue = queuedRequests
            .Where(queuedRequest => !IsSamePickupIdentity(queuedRequest, request))
            .ToList();

        deduplicatedQueue.Insert(
            Math.Min(firstMatchingIndex, deduplicatedQueue.Count),
            mergedRequest
        );

        _waitingPickupRequests.Clear();
        foreach (var queuedRequest in deduplicatedQueue)
        {
            _waitingPickupRequests.Enqueue(queuedRequest);
        }

        return true;
    }

    private static bool IsSamePickupIdentity(PickupRequest left, PickupRequest right) =>
        left.Floor == right.Floor && left.Direction == right.Direction;

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

    public CommandResult RemoveElevator(int elevatorId)
    {
        var elevator = _elevatorFleet.FirstOrDefault(existingElevator =>
            existingElevator.Id.Value == elevatorId
        );
        if (elevator is null)
        {
            return CommandResult.Fail($"Elevator {elevatorId} not found.");
        }

        _elevatorFleet.Remove(elevator);
        return CommandResult.Ok($"Elevator {elevatorId} removed.");
    }
}
