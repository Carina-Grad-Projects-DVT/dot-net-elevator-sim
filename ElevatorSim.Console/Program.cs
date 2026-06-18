using ElevatorSim.Application.Controllers;
using ElevatorSim.Application.Factories;
using ElevatorSim.Application.Models;
using ElevatorSim.Application.Services;
using ElevatorSim.Domain.Enums;
using ElevatorSim.Domain.ValueObjects;
using ElevatorSim.Infrastructure.Presenters;

const int defaultMinimumFloor = -1;
const int defaultMaximumFloor = 5;

var app = new ElevatorConsoleApp(defaultMinimumFloor, defaultMaximumFloor);
app.Run();

internal class ElevatorConsoleApp
{
    private ElevatorSystemController _controller;
    private readonly ConsoleUIPresenter _presenter;
    private int _minimumFloor;
    private int _maximumFloor;
    private string _previousStatusMessage;

    public ElevatorConsoleApp(int minimumFloor, int maximumFloor)
    {
        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;
        _presenter = new ConsoleUIPresenter(minimumFloor, maximumFloor);
        _controller = CreateDefaultSystemController();
        _previousStatusMessage = "Use help for available commands.";
    }

    public void Run()
    {
        _previousStatusMessage = FormatCommandResult(ConfigureSystemFromPrompts());
        RenderDashboard();

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

            if (ExecuteCommandSafely(parts))
            {
                break;
            }

            RenderDashboard();
        }
    }

    private ElevatorSystemController CreateDefaultSystemController()
    {
        // Default passenger capacity
        var defaultFleetConfiguration = BuildPassengerFleetConfiguration(
            elevatorCount: 2,
            minimumFloor: _minimumFloor,
            maximumFloor: _maximumFloor,
            maximumPassengerCapacity: 4
        );

        return CreateSystemController(defaultFleetConfiguration);
    }

    private static ElevatorSystemController CreateSystemController(
        IEnumerable<ElevatorConfiguration> fleetConfiguration
    )
    {
        var factory = new ElevatorFactory();
        var dispatchService = new NearestElevatorDispatchService();
        var elevatorFleet = factory.CreateMany(fleetConfiguration);
        return new ElevatorSystemController(elevatorFleet, dispatchService);
    }

    private CommandResult ConfigureSystemFromPrompts()
    {
        Console.Clear();
        Console.WriteLine("Welcome to the elevator simulator :) Building setup time");
        Console.WriteLine("Press Enter to keep the default value in brackets or enter new value.");

        var minimumFloor = ReadIntWithDefaultValue(
            prompt: "Please enter the minumum floor with elevator access(Negative for basement ex -1)",
            defaultValue: _minimumFloor
        );
        var maximumFloor = ReadIntWithDefaultValue(
            prompt: "Please enter the maximum floor with elevator access",
            defaultValue: _maximumFloor
        );

        if (minimumFloor > maximumFloor)
        {
            return CommandResult.Fail("Minimum floor cannot be greater than maximum floor.");
        }

        var passengerElevatorCount = ReadIntWithDefaultValue(
            // passenger elevator by default
            prompt: "Please indicate the amount of elevators in the building",
            defaultValue: 2,
            minimumValue: 1
        );
        var maximumPassengerCapacity = ReadIntWithDefaultValue(
            prompt: "Please enter the max elevator passenger capacity",
            defaultValue: 8,
            minimumValue: 1
        );

        var fleetConfiguration = BuildPassengerFleetConfiguration(
            elevatorCount: passengerElevatorCount,
            minimumFloor: minimumFloor,
            maximumFloor: maximumFloor,
            maximumPassengerCapacity: maximumPassengerCapacity
        );

        _minimumFloor = minimumFloor;
        _maximumFloor = maximumFloor;
        _presenter.UpdateBuildingConfig(minimumFloor, maximumFloor);
        _controller = CreateSystemController(fleetConfiguration);

        return CommandResult.Ok(
            $"Initialised floors {FormatFloorLabel(minimumFloor)} to {FormatFloorLabel(maximumFloor)} with {passengerElevatorCount} passenger elevator(s), each capacity {maximumPassengerCapacity}."
        );
    }

    private static List<ElevatorConfiguration> BuildPassengerFleetConfiguration(
        int elevatorCount,
        int minimumFloor,
        int maximumFloor,
        int maximumPassengerCapacity
    )
    {
        var startingFloor = ResolveDefaultStartingFloor(minimumFloor, maximumFloor);
        var configuration = new List<ElevatorConfiguration>(elevatorCount);

        for (var i = 0; i < elevatorCount; i++)
        {
            configuration.Add(
                new PassengerElevatorConfiguration
                {
                    MinimumFloor = minimumFloor,
                    MaximumFloor = maximumFloor,
                    StartingFloor = startingFloor,
                    MaximumPassengerCapacity = maximumPassengerCapacity,
                }
            );
        }

        return configuration;
    }

    // Helper so starting floor is always ground when possible
    // Using my apartment elevator's rule where it rests at ground level when not in use. Thus starting at ground floor.
    private static int ResolveDefaultStartingFloor(int minimumFloor, int maximumFloor)
    {
        if (minimumFloor <= 0 && maximumFloor >= 0)
        {
            return 0;
        }

        return minimumFloor;
    }

    private static int ReadIntWithDefaultValue(
        string prompt,
        int defaultValue,
        int? minimumValue = null,
        int? maximumValue = null
    )
    {
        while (true)
        {
            var rangeText = BuildRangeText(minimumValue, maximumValue);
            Console.Write($"{prompt}{rangeText} [{defaultValue}]: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (
                    TryValidateRange(defaultValue, minimumValue, maximumValue, out var defaultError)
                )
                {
                    return defaultValue;
                }

                Console.WriteLine(defaultError);
                continue;
            }

            if (!int.TryParse(input, out var value))
            {
                Console.WriteLine($"Invalid number: '{input}'.");
                continue;
            }

            if (!TryValidateRange(value, minimumValue, maximumValue, out var validationError))
            {
                Console.WriteLine(validationError);
                continue;
            }

            return value;
        }
    }

    private static bool TryValidateRange(
        int value,
        int? minimumValue,
        int? maximumValue,
        out string error
    )
    {
        if (minimumValue is int min && value < min)
        {
            error = $"Value must be {min} or greater.";
            return false;
        }

        if (maximumValue is int max && value > max)
        {
            error = $"Value must be {max} or less.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string BuildRangeText(int? minimumValue, int? maximumValue)
    {
        if (minimumValue is int min && maximumValue is int max)
        {
            return $" ({min}..{max})";
        }

        if (minimumValue is int minimumOnly)
        {
            return $" (>= {minimumOnly})";
        }

        if (maximumValue is int maximumOnly)
        {
            return $" (<= {maximumOnly})";
        }

        return string.Empty;
    }

    private static string[] ParseCommand(string input) =>
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private bool ExecuteCommand(string[] parts)
    {
        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
            case "help":
                _previousStatusMessage = BuildHelpMessage();
                break;

            case "status":
                _previousStatusMessage = BuildStatusSummaryMessage();
                break;

            case "request":
            case "pickup":
                _previousStatusMessage = ExecutePickup(parts);
                break;

            case "step":
                _previousStatusMessage = ExecuteStep(parts);
                break;

            case "restart":
                _previousStatusMessage = FormatCommandResult(ConfigureSystemFromPrompts());
                break;

            case "exit":
            case "quit":
            case "q":
                return true;

            default:
                _previousStatusMessage = "[error] Unknown command. Use 'help'.";
                break;
        }

        return false;
    }

    private bool ExecuteCommandSafely(string[] parts)
    {
        try
        {
            return ExecuteCommand(parts);
        }
        catch (Exception exception)
        {
            _previousStatusMessage = $"[error] {exception.Message}";
            return false;
        }
    }

    private string ExecutePickup(string[] parts)
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
            return $"[error] {error}";
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

        return FormatDispatchResult(_controller.RequestPickup(pickupRequest));
    }

    private string ExecuteStep(string[] parts)
    {
        if (parts.Length == 1)
        {
            return FormatCommandResult(_controller.StepAll());
        }

        if (!TryReadIntArg(parts, out var ticks, out var error))
        {
            return $"[error] {error}";
        }

        return FormatCommandResult(AdvanceSystemTicks(ticks));
    }

    private static string FormatCommandResult(CommandResult result)
    {
        var prefix = result.Success ? "[ok]" : "[error]";
        return $"{prefix} {result.Message}";
    }

    private static string FormatDispatchResult(DispatchResult result)
    {
        var prefix = result.IsSuccess ? "[ok]" : "[error]";
        var etaSuffix = result.EstimatedArrivalTicks is int estimatedArrivalTicks
            ? $" ETA: {estimatedArrivalTicks} tick(s)."
            : string.Empty;
        return $"{prefix} {result.Message}{etaSuffix}";
    }

    private CommandResult AdvanceSystemTicks(int tickCount)
    {
        if (tickCount < 1)
        {
            return CommandResult.Fail("Tick count must be 1 or more.");
        }

        for (var i = 0; i < tickCount; i++)
        {
            var stepResult = _controller.StepAll();
            if (!stepResult.Success)
            {
                return stepResult;
            }
        }

        return CommandResult.Ok($"Advanced all elevators by {tickCount} tick(s).");
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
                error = "Use 'up' or 'down' for direction or a waiting passenger count.";
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

    private void RenderDashboard()
    {
        var fleetStatus = _controller.GetFleetStatus();
        var waitingByFloor = _controller.GetQueuedWaitingCountsByFloor();
        _presenter.RenderConsoleUI(fleetStatus, waitingByFloor, _previousStatusMessage);
    }

    private string BuildStatusSummaryMessage()
    {
        var fleetStatus = _controller.GetFleetStatus();
        var movingCount = fleetStatus.Count(status => status.MotionState == MotionState.Moving);
        var queuedCount = _controller.QueuedPickupRequestCount;
        return $"[ok] Elevators: {fleetStatus.Count}, moving: {movingCount}, queued pickup requests: {queuedCount}.";
    }

    private static ElevatorDirection ParseDirection(string directionText) =>
        directionText.ToLowerInvariant() switch
        {
            "up" or "u" => ElevatorDirection.Up,
            "down" or "d" => ElevatorDirection.Down,
            _ => ElevatorDirection.None,
        };

    private static string BuildHelpMessage() =>
        "[ok] Commands: help, status, restart, request <floor> [up|down] [waitingPassengers], step [ticks], exit";

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
}
