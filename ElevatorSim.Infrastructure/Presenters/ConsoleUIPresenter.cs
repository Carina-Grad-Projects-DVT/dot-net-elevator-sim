using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Enums;

namespace ElevatorSim.Infrastructure.Presenters;

public class ConsoleUIPresenter
{
    private const int CellWidth = 22;
    private const string DashDivider =
        "=========================================================================";

    private int _minimumFloor;
    private int _maximumFloor;

    public ConsoleUIPresenter(int minimumFloor, int maximumFloor)
    {
        UpdateBuildingConfig(minimumFloor, maximumFloor);
    }

    public void UpdateBuildingConfig(int minimumFloor, int maximumFloor)
    {
        if (minimumFloor > maximumFloor)
        {
            throw new ArgumentException("Minimum floor cannot be greater than maximum floor.");
        }

        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;
    }

    public void RenderConsoleUI(
        IReadOnlyCollection<ElevatorStatus> elevatorStatuses,
        IReadOnlyDictionary<int, int> queuedOnFloor,
        string? previousStatusMessage = null
    )
    {
        ArgumentNullException.ThrowIfNull(elevatorStatuses);
        ArgumentNullException.ThrowIfNull(queuedOnFloor);

        var requestedElevatorStatuses = elevatorStatuses
            .OrderBy(elevatorStatus => elevatorStatus.ElevatorId)
            .ToList();

        Console.Clear();
        Console.WriteLine(DashDivider);
        Console.WriteLine(
            "                           ELEVATOR SIMULATION                           "
        );
        Console.WriteLine(DashDivider);
        RenderBuildingGrid(requestedElevatorStatuses, queuedOnFloor);
        Console.WriteLine(DashDivider);
        RenderStatus(previousStatusMessage);
        Console.WriteLine(DashDivider);
        RenderTips();
    }

    private void RenderBuildingGrid(
        IReadOnlyList<ElevatorStatus> requestedElevatorStatuses,
        IReadOnlyDictionary<int, int> queuedOnFloor
    )
    {
        for (var floor = _maximumFloor; floor >= _minimumFloor; floor--)
        {
            var elevatorCells = requestedElevatorStatuses
                .Select(elevatorStatus =>
                    elevatorStatus.CurrentFloor == floor
                        ? BuildElevatorCell(elevatorStatus)
                        : string.Empty
                )
                .Select(cell => cell.PadRight(CellWidth))
                .ToArray();

            var queueDisplay = BuildQueueDisplay(queuedOnFloor, floor);
            var floorLabel = FormatFloorLabel(floor).PadLeft(2);
            Console.WriteLine(
                $"Floor {floorLabel} | {string.Join(" | ", elevatorCells)} | Queue: {queueDisplay}"
            );
        }
    }

    private static string BuildElevatorCell(ElevatorStatus elevatorStatus)
    {
        var directionSymbol = elevatorStatus.Direction switch
        {
            ElevatorDirection.Up => "▲",
            ElevatorDirection.Down => "▼",
            _ => "■",
        };

        var loadSummary = elevatorStatus.ElevatorType switch
        {
            ElevatorType.Passenger =>
                $"{elevatorStatus.CurrentPassengers ?? 0}/{elevatorStatus.MaximumCapacity ?? 0}",
            ElevatorType.Freight =>
                $"{elevatorStatus.CurrentLoadKg?.ToString("0.##") ?? "0"}/{elevatorStatus.MaximumLoadKg?.ToString("0.##") ?? "0"}kg",
            _ => "-",
        };

        return $"[E{elevatorStatus.ElevatorId}:{directionSymbol}({loadSummary})]";
    }

    private static string BuildQueueDisplay(IReadOnlyDictionary<int, int> queuedOnFloor, int floor)
    {
        if (!queuedOnFloor.TryGetValue(floor, out var waitingCount) || waitingCount < 1)
        {
            return string.Empty;
        }

        // Show a max of 5 dots to save ui space
        var waitingDotCount = Math.Min(waitingCount, 5);
        var dots = string.Concat(Enumerable.Repeat("●", waitingDotCount));
        var truncated = waitingCount > waitingDotCount ? "…" : string.Empty;
        return $"{dots}{truncated} ({waitingCount} waiting)";
    }

    private static string FormatFloorLabel(int floor)
    {
        if (floor < 0)
        {
            return $"B{Math.Abs(floor)}";
        }

        if (floor == 0)
        {
            return "G";
        }

        return floor.ToString();
    }

    private static void RenderStatus(string? previousStatusMessage)
    {
        if (string.IsNullOrWhiteSpace(previousStatusMessage))
        {
            Console.WriteLine("Status: ready");
            return;
        }

        Console.WriteLine($"Status: {previousStatusMessage}");
    }

    private static void RenderTips()
    {
        Console.WriteLine(
            "Commands: help, status, restart, request <floor> [up|down] [waiting], step [ticks], exit"
        );
        Console.WriteLine("Legend : ▲ - Up, ▼ - Down, ■ - Stationary, ● - Person in queue");
    }
}
