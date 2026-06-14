using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Entities;
using ElevatorSim.Domain.ValueObjects;

namespace ElevatorSim.Application.Controllers;

/// <summary>
/// Acts as a middle layer between Elevator commands and ui
/// </summary>
public sealed class ElevatorController
{
    private readonly Elevator _elevator;

    /// <summary>
    /// Initializes a new controller for a single elevator instance.
    /// </summary>
    /// <param name="elevator">Elevator aggregate to control.</param>
    public ElevatorController(Elevator elevator)
    {
        _elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
    }

    /// <summary>
    /// Gets a snapshot of current elevator state.
    /// </summary>
    public ElevatorStatus GetStatus() => ElevatorStatus.From(_elevator);

    /// <summary>
    /// Queues a stop by floor value.
    /// </summary>
    /// <param name="floor">Requested floor.</param>
    public CommandResult RequestStop(int floor) =>
        Execute(() => _elevator.RequestStop(floor), $"Requested stop at floor {floor}.");

    /// <summary>
    /// Boards passengers into the elevator.
    /// </summary>
    /// <param name="passengerCount">Number of boarding passengers.</param>
    public CommandResult Board(int passengerCount) =>
        Execute(
            () => _elevator.Board(new PassengerCount(passengerCount)),
            $"Boarded {passengerCount} passenger(s)."
        );

    /// <summary>
    /// Let's passengers off from the elevator.
    /// </summary>
    /// <param name="passengerCount">Number of disembarking passengers.</param>
    public CommandResult Disembark(int passengerCount) =>
        Execute(
            () => _elevator.Disembark(new PassengerCount(passengerCount)),
            $"Disembarked {passengerCount} passenger(s)."
        );

    /// <summary>
    /// Advances the elevator simulation by one tick.
    /// </summary>
    public CommandResult Step() => Execute(_elevator.Step, "Advanced simulation by 1 tick.");

    /// <summary>
    /// Advances the elevator simulation by a number of ticks.
    /// </summary>
    /// <param name="tickCount">Tick count to process.</param>
    public CommandResult AdvanceTicks(int tickCount) =>
        Execute(
            () => _elevator.AdvanceTicks(tickCount),
            $"Advanced simulation by {tickCount} tick(s)."
        );

    // avoid repeating try/catch in every controller
    private static CommandResult Execute(Action action, string successMessage)
    {
        try
        {
            action();
            return CommandResult.Ok(successMessage);
        }
        catch (Exception exception)
        {
            return CommandResult.Fail(exception.Message);
        }
    }
}
