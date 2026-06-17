using ElevatorSim.Application.Controllers;
using ElevatorSim.Application.Factories;
using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;

const int minimumFloor = -1;
const int maximumFloor = 10;

var app = new ElevatorConsoleApp(minimumFloor, maximumFloor);
app.Run();

internal class ElevatorConsoleApp
{
    private readonly ElevatorSystemController _controller;
    private readonly int _minimumFloor;
    private readonly int _maximumFloor;

    public ElevatorConsoleApp(int minimumFloor, int maximumFloor)
    {
        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;

        var factory = new ElevatorFactory();
        // 2 elevator fleet
        var fleetConfiguration = new ElevatorConfiguration[]
        {
            new PassengerElevatorConfiguration
            {
                MinimumFloor = minimumFloor,
                MaximumFloor = maximumFloor,
                StartingFloor = 0,
                MaximumPassengerCapacity = 8,
            },
            new PassengerElevatorConfiguration
            {
                MinimumFloor = minimumFloor,
                MaximumFloor = maximumFloor,
                StartingFloor = 6,
                MaximumPassengerCapacity = 10,
            },
        };

        var elevatorFleet = factory.CreateMany(fleetConfiguration);
        var dispatchService = new NearestElevatorDispatchService();
        _controller = new ElevatorSystemController(elevatorFleet, dispatchService);
    }

    public void Run()
    {
        Console.WriteLine("ElevatorSim Console");
        PrintHelp();
        PrintStatus();

        while (true)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine();
            if (input is null)
            {
                Console.WriteLine();
                break;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var parts = ParseCommand(input);
            if (parts.Length == 0)
            {
                continue;
            }

            if (ExecuteCommand(parts))
            {
                break;
            }

            PrintStatus();
        }
    }

    private static string[] ParseCommand(string input) =>
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private bool ExecuteCommand(string[] parts)
    {
        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
            case "help":
                PrintHelp();
                break;

            case "status":
                PrintStatus();
                break;

            case "request":
            case "pickup":
                ExecutePickup(parts);
                break;

            case "step":
                ExecuteStep(parts);
                break;

            case "exit":
            case "quit":
            case "q":
                return true;

            default:
                Console.WriteLine("Unknown command.");
                PrintHelp();
                break;
        }

        return false;
    }

    private void ExecutePickup(string[] parts)
    {
        if (
            !TryParsePickupRequestArgs(
                parts,
                out var floor,
                out var direction,
                out var waitingPassengerCount,
                out var error
            )
        )
        {
            Console.WriteLine(error);
            return;
        }

        var pickupRequest =
            direction == ElevatorDirection.None
                ? PickupRequest.Create(floor, waitingPassengerCount, _minimumFloor, _maximumFloor)
                : PickupRequest.Create(
                    floor,
                    waitingPassengerCount,
                    direction,
                    _minimumFloor,
                    _maximumFloor
                );

        PrintDispatchResult(_controller.RequestPickup(pickupRequest));
    }

    private void ExecuteStep(string[] parts)
    {
        if (parts.Length == 1)
        {
            PrintResult(_controller.StepAll());
            return;
        }

        if (!TryReadIntArg(parts, out var ticks, out var error))
        {
            Console.WriteLine(error);
            return;
        }

        PrintResult(AdvanceSystemTicks(ticks));
    }

    private void PrintResult(CommandResult result)
    {
        var prefix = result.Success ? "[ok]" : "[error]";
        Console.WriteLine($"{prefix} {result.Message}");
    }

    private void PrintDispatchResult(DispatchResult result)
    {
        var prefix = result.IsSuccess ? "[ok]" : "[error]";
        var etaSuffix = result.EstimatedArrivalTicks is int estimatedArrivalTicks
            ? $" ETA: {estimatedArrivalTicks} tick(s)."
            : string.Empty;

        Console.WriteLine($"{prefix} {result.Message}{etaSuffix}");
    }

    private void PrintStatus()
    {
        var queuedCount = _controller.QueuedPickupRequestCount;
        if (queuedCount > 0)
        {
            Console.WriteLine($"Waiting pickup requests: {queuedCount}");
        }

        var fleetStatus = _controller.GetFleetStatus();
        foreach (var status in fleetStatus.OrderBy(elevatorStatus => elevatorStatus.ElevatorId))
        {
            var capacitySummary = status.ElevatorType switch
            {
                ElevatorType.Passenger =>
                    $"passengers: {status.CurrentPassengers ?? 0}/{status.MaximumCapacity ?? 0}",
                ElevatorType.Freight =>
                    $"load: {status.CurrentLoadKg?.ToString("0.##") ?? "0"}/{status.MaximumLoadKg?.ToString("0.##") ?? "0"} kg",
                _ => "capacity: -",
            };

            var capacityState = status.ElevatorType switch
            {
                ElevatorType.Passenger => $"at capacity: {status.IsAtCapacity ?? false}",
                ElevatorType.Freight => $"at load capacity: {status.IsAtLoadCapacity ?? false}",
                _ => "capacity state: -",
            };

            Console.WriteLine(
                $"E{status.ElevatorId} [{status.ElevatorType}] | floor: {status.CurrentFloorDisplay} ({status.CurrentFloor}) | "
                    + $"dir: {status.Direction} | motion: {status.MotionState} | doors: {status.DoorState} | "
                    + $"{capacitySummary} | {capacityState} | pending stops: {status.PendingStopCount}"
            );
        }
    }

    private static bool TryReadIntArg(string[] parts, out int value, out string error)
    {
        value = 0;
        error = "Expected one numeric argument.";

        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out value))
        {
            error = $"Invalid number: '{parts[1]}'.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    // Parse and validate the arguments for request & pickup commands
    private static bool TryParsePickupRequestArgs(
        string[] parts,
        out int floor,
        out ElevatorDirection direction,
        out int waitingPassengerCount,
        out string error
    )
    {
        floor = 0;
        direction = ElevatorDirection.None;
        waitingPassengerCount = 1;
        error = "Expected: request <floor> [up|down] [waitingPassengers].";

        if (parts.Length is < 2 or > 4)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out floor))
        {
            error = $"Invalid floor: '{parts[1]}'.";
            return false;
        }

        var directionGiven = false;
        var waitingPassengerCountSpecified = false;
        for (var i = 2; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out var parsedWaitingPassengerCount))
            {
                if (waitingPassengerCountSpecified)
                {
                    error = "Waiting passenger count can only be provided once.";
                    return false;
                }

                if (parsedWaitingPassengerCount < 1)
                {
                    error = "Waiting passenger count must be 1 or more.";
                    return false;
                }

                waitingPassengerCount = parsedWaitingPassengerCount;
                waitingPassengerCountSpecified = true;
                continue;
            }

            var parsedDirection = ParseDirection(parts[i]);
            if (parsedDirection == ElevatorDirection.None)
            {
                error = "Use 'up' or 'down' for direction or a waiting passenger count";
                return false;
            }

            if (directionGiven)
            {
                error = "Direction can only be given once.";
                return false;
            }

            direction = parsedDirection;
            directionGiven = true;
        }

        error = string.Empty;
        return true;
    }

    private CommandResult AdvanceSystemTicks(int tickCount)
    {
        if (tickCount < 1)
        {
            return CommandResult.Fail("Tick count must be 1 or more.");
        }

        for (var i = 0; i < tickCount; i++)
        {
            var result = _controller.StepAll();
            if (!result.Success)
            {
                return result;
            }
        }

        return CommandResult.Ok($"Advanced all elevators by {tickCount} tick(s).");
    }

    private static ElevatorDirection ParseDirection(string directionText) =>
        directionText.ToLowerInvariant() switch
        {
            "up" or "u" => ElevatorDirection.Up,
            "down" or "d" => ElevatorDirection.Down,
            _ => ElevatorDirection.None,
        };

    private static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  help");
        Console.WriteLine("  status");
        Console.WriteLine("  request <floor> [up|down] [waitingPassengers]");
        Console.WriteLine("  pickup <floor> [up|down] [waitingPassengers]");
        Console.WriteLine("  step [ticks]");
        Console.WriteLine("  exit");
    }
}
