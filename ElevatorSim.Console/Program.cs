using ElevatorSim.Application.Controllers;
using ElevatorSim.Application.Factories;
using ElevatorSim.Application.Models;
using ElevatorSim.Domain.Enums;

var factory = new ElevatorFactory();
var elevatorConfiguration = new ElevatorConfiguration
{
    Id = 1,
    Type = ElevatorType.Passenger,
    MinimumFloor = -1,
    MaximumFloor = 10,
    StartingFloor = 0,
    MaximumPassengerCapacity = 8,
};

var elevator = factory.Create(elevatorConfiguration);
var controller = new ElevatorController(elevator);

Console.WriteLine("ElevatorSim Console");
PrintHelp();
PrintStatus();

while (true)
{
    Console.Write("\n> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    var parts = input.Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
    );
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
            if (!TryReadIntArg(parts, out var floor, out var requestError))
            {
                Console.WriteLine(requestError);
                break;
            }

            PrintResult(controller.RequestStop(floor));
            break;

        case "board":
            if (!TryReadIntArg(parts, out var boardingCount, out var boardError))
            {
                Console.WriteLine(boardError);
                break;
            }

            PrintResult(controller.Board(boardingCount));
            break;

        case "disembark":
            if (!TryReadIntArg(parts, out var disembarkCount, out var disembarkError))
            {
                Console.WriteLine(disembarkError);
                break;
            }

            PrintResult(controller.Disembark(disembarkCount));
            break;

        case "step":
            if (parts.Length == 1)
            {
                PrintResult(controller.Step());
                break;
            }

            if (!TryReadIntArg(parts, out var ticks, out var stepError))
            {
                Console.WriteLine(stepError);
                break;
            }

            PrintResult(controller.AdvanceTicks(ticks));
            break;

        case "exit":
            return;

        default:
            Console.WriteLine("Unknown command.");
            PrintHelp();
            break;
    }

    PrintStatus();
}

void PrintResult(CommandResult result)
{
    var prefix = result.Success ? "[ok]" : "[error]";
    Console.WriteLine($"{prefix} {result.Message}");
}

void PrintStatus()
{
    var status = controller.GetStatus();

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

bool TryReadIntArg(string[] parts, out int value, out string error)
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

void PrintHelp()
{
    Console.WriteLine("Commands:");
    Console.WriteLine("  help");
    Console.WriteLine("  status");
    Console.WriteLine("  request <floor>");
    Console.WriteLine("  board <count>");
    Console.WriteLine("  disembark <count>");
    Console.WriteLine("  step [ticks]");
    Console.WriteLine("  exit");
}
