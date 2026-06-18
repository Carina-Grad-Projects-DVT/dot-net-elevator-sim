# C# DOT NET Elevator Simulator

Elevator Simulator implemented following SOLID Principles and Clean Architecture.

## Setup Instructions

```bash
git clone git@github.com:Carina-Grad-Projects-DVT/dot-net-elevator-sim.git
cd ElevatorSim
dotnet restore
```

## How to Run

```bash
dotnet run --project ElevatorSim.Console/ElevatorSim.Console.csproj
```

At startup, configure the building (floor range, elevator count, capacity). Use `help` command in the console to see available commands.

## Assumptions

- Single building, configurable floors – Users set floor range and elevator count at runtime (default: -1 to 5, 2 elevators of capacity 4).
- Passenger elevators only – Fleet uses only passenger elevators. Freight elevators basics are set up but not implemented.
- Tick-based simulation – Elevators move one floor per tick and auto-tick loop runs up to 20 ticks when elevators are active.
- Nearest-available dispatch – Requests assigned to closest stationary elevator with no pending stops, requests exceeding max capacity are rejected and otherwise queued.
- Request merging – Multiple requests to the same floor with the same direction merge (passenger counts accumulate).

## Repository Structure

### .gitignore

The `.gitignore` file excludes the following from version control:

- **Build outputs:** `bin/`, `obj/` – compiler and runtime artifacts
- **IDE files:** `.vs/` (Visual Studio), `.idea/` (Rider), `.vscode/` (VS Code) – IDE-specific settings and caches
- **User files:** `*.user`, `*.suo`, `*.userosscache` – per-developer Visual Studio preferences
- **Test results:** `TestResults/`, `coverage/`, `*.coverage*` – generated test execution and code coverage reports
- **NuGet packages:** `*.nupkg`, `packages/` – dependency packages (restored via `dotnet restore`)
- **Logs:** `*.log` – application and build logs
- **OS files:** `.DS_Store`, `Thumbs.db` – macOS and Windows metadata files

## Notes

- feature/domain-starter-logic only has one commit due to a mistake with gitignore. The true commits for this branch can be seen on feature/initial-domain-logic
- Freight elevator types are partially defined but creation is not supported.
- Requests for passenger groups larger than any single elevator's capacity are rejected (not split).
