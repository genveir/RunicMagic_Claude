# UI

## Layout

The UI is split into two panels:

- **Canvas (top)** — a 2D view of the world. Entities are rendered as labelled rectangles, directly mirroring the world model's axis-aligned rectangle representation. Spatial relationships (containment, touch, proximity) are visible at a glance.
- **Terminal (bottom)** — a REPL for spell input and world event output. The player types rune strings and hits Enter; results and world events are echoed back as text.

The canvas is driven by a `requestAnimationFrame` loop that consumes the latest entity snapshot from the SSE stream. The terminal receives text and prompt updates from the same stream.

## Player interface

**`IPlayerViewInterface`** — called by the View layer. Methods: `RegisterInput(string)`, `SetCaster`, `MoveCaster`, `SetPointingDirection`, `SetIndicateTarget` — all return `Task` (fire-and-forget; results are delivered via SSE).

Player actions are enqueued rather than executed immediately. The game loop drains the queue each tick, collects any text produced, and pushes output via SSE.

## Output models

Two models flow through the output pipeline:

**`TickResult`** — the internal game-loop product, passed from `GameLoopService` to `IWorldTickSink`: `Text` (terminal lines), `Entities` (canvas snapshot), `CasterData` (caster stats used to derive the prompt string).

**`ViewUpdateModel`** — the SSE payload, produced by `ViewUpdateFormattingService` from a `TickResult`: `Text`, `Entities`, and `Prompt` (the formatted prompt string derived from `CasterData`). This is what the client receives.

`ViewUpdateFormattingService` implements `IWorldTickSink` and is the bridge between the two: it formats `CasterData` into the prompt string and writes a `ViewUpdateModel` to `SseConnectionManager`.

`EntityRenderingModel` carries everything the canvas needs and nothing else:

- `X`, `Y`, `Width`, `Height` — position and dimensions in world coordinates
- `Angle` — rotation in radians
- `Label` — display name
- `Flags` — `HasLife`, `HasAgency`, `IsTranslucent` — used for visual styling; the canvas has no knowledge of what these mean to the magic system
- `IsCaster` — whether this entity is the current caster (highlighted on canvas)
- `PointingEndX`, `PointingEndY` — endpoint of the caster's pointing direction arrow (null if not pointing)
- `IsIndicateTarget` — whether this entity is the current indicate target
- `IndicateEndX`, `IndicateEndY` — endpoint of the indicate arrow (null if not set)

The canvas is responsible for mapping world coordinates to screen coordinates. The game logic never knows or cares about screen size.

## Output pipeline

All output flows through the game loop:

1. Player submits a command or canvas action → HTTP POST → 204 (no body) → action enqueued in `PlayerService`
2. Game loop (60 FPS) drains the queue each tick → executes actions → collects events via `EventTracker` → packages into `TickResult`
3. `GameLoopService` pushes `TickResult` to `IWorldTickSink` (`ViewUpdateFormattingService`)
4. `ViewUpdateFormattingService` derives the prompt string from `CasterData`, produces a `ViewUpdateModel`, and passes it to `SseConnectionManager`
5. `SseConnectionManager` writes the `ViewUpdateModel` to all connected clients' SSE channels
6. Client `EventSource` receives the event → writes text to terminal, updates prompt, stores entities for next `requestAnimationFrame`

The game loop only pushes when the queue was non-empty or when motion effects advanced world state. Idle ticks produce no SSE traffic.

On initial SSE connection, the server immediately sends the current world state (entities + current prompt, no text) so the canvas and prompt are populated without waiting for a player action.

## Implementation notes

- ASP.NET Core Web API backend + static HTML/JS frontend.
- Terminal is xterm.js. Canvas is an inline SVG element.
- `GET /events` — SSE endpoint; client subscribes on page load. Each event is a JSON-serialised `ViewUpdateModel`.
- `POST /command`, `POST /pick-caster`, etc. — all return 204 with no body.
- Canvas SVG is rebuilt on each `requestAnimationFrame` tick when new entity data has arrived.
- Canvas is SVG for the prototype (DOM hit-testing comes for free, sufficient for a small number of entities).

## Future: IDE functionality

Complex spells may eventually demand tooling beyond a raw REPL — things like a rune palette, expression tree visualisation, inline type hints, or a spell library browser. None of this is in scope for the first version. The terminal is sufficient to get spells working and observable.
