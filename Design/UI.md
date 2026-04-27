# UI

## Layout

The UI is split into two panels:

- **Canvas (top)** — a 2D view of the world. Entities are rendered as labelled rectangles, directly mirroring the world model's axis-aligned rectangle representation. Spatial relationships (containment, touch, proximity) are visible at a glance.
- **Terminal (bottom)** — a REPL for spell input and world event output. The player types rune strings and hits Enter; results and world events are echoed back as text.

The canvas is driven by a `requestAnimationFrame` loop that consumes the latest entity snapshot from the SSE stream. The terminal receives text and prompt updates from the same stream.

## Player interface

Two interfaces split the concerns:

**`IPlayerViewInterface`** — called by the View layer. Methods: `RegisterInput(string)`, `SetCaster`, `MoveCaster`, `SetPointingDirection`, `SetIndicateTarget` — all return `Task` (fire-and-forget; results are delivered via SSE). Also exposes `Prompt` for the current terminal prompt string.

**`IPlayerOutputSink`** — called by internal services during command processing to accumulate output: `SendText(string)`.

Player actions are enqueued rather than executed immediately. The game loop drains the queue each tick, collects any text produced, and pushes a `CommandResult` via SSE.

`CommandResult` bundles everything produced during one drain into two lists plus a prompt: `Text` (terminal lines), `Entities` (canvas snapshot), and `Prompt` (current prompt string). It is pushed via SSE — there is no HTTP response body for player actions.

`EntityRenderingModel` carries everything the canvas needs and nothing else:

- `X`, `Y`, `Width`, `Height` — position and dimensions in world coordinates
- `Label` — display name
- `Flags` — flags (e.g. `HasLife`, `HasAgency`) used for visual styling; the canvas has no knowledge of what these mean to the magic system

The canvas is responsible for mapping world coordinates to screen coordinates. The game logic never knows or cares about screen size.

## Output pipeline

All output flows through the game loop:

1. Player submits a command or canvas action → HTTP POST → 204 (no body) → action enqueued in `PlayerService`
2. Game loop (60 FPS) drains the queue each tick → executes actions → collects text via `IPlayerOutputSink` → packages into `CommandResult`
3. `GameLoopService` pushes the result to `IWorldTickSink` (`SseConnectionManager`)
4. `SseConnectionManager` writes the result to all connected clients' SSE channels
5. Client `EventSource` receives the event → writes text to terminal, updates prompt, stores entities for next `requestAnimationFrame`

The game loop only pushes when the queue was non-empty (or, in future, when world state changed due to AI or motion effects). Idle ticks produce no SSE traffic.

On initial SSE connection, the server immediately sends the current world state (entities + current prompt, no text) so the canvas and prompt are populated without waiting for a player action.

## Implementation notes

- ASP.NET Core Web API backend + static HTML/JS frontend.
- Terminal is xterm.js. Canvas is an inline SVG element.
- `GET /events` — SSE endpoint; client subscribes on page load. Each event is a JSON-serialised `CommandResult`.
- `POST /command`, `POST /pick-caster`, etc. — all return 204 with no body.
- Canvas SVG is rebuilt on each `requestAnimationFrame` tick when new entity data has arrived.
- Canvas is SVG for the prototype (DOM hit-testing comes for free, sufficient for a small number of entities).

## Future: IDE functionality

Complex spells may eventually demand tooling beyond a raw REPL — things like a rune palette, expression tree visualisation, inline type hints, or a spell library browser. None of this is in scope for the first version. The terminal is sufficient to get spells working and observable.
