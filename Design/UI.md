# UI

## Layout

The UI is split into two panels:

- **Canvas (top)** — a 2D view of the world. Entities are rendered as labelled rectangles, directly mirroring the world model's axis-aligned rectangle representation. Spatial relationships (containment, touch, proximity) are visible at a glance.
- **Terminal (bottom)** — a REPL for spell input and world event output. The player types rune strings and hits Enter; results and world events are echoed back as text.

The canvas redraws after each command, in the same render pass as the terminal output flush.

## Player interface

Two interfaces split the concerns:

**`IPlayerViewInterface`** — called by the View layer. One method: `RegisterInput(string) → CommandResult`. Also exposes `Prompt` for the terminal prompt string.

**`IPlayerOutputSink`** — called by internal services during command processing to accumulate output: `SendText(string)` and `SendEntity(EntityRenderingModel)`.

`CommandResult` bundles everything produced during a single command into two lists: `Text` (terminal lines) and `Entities` (canvas snapshot). It is returned directly from `RegisterInput` — there is no push/callback mechanism.

`EntityRenderingModel` carries everything the canvas needs and nothing else:

- `X`, `Y`, `Width`, `Height` — position and dimensions in world coordinates
- `Label` — display name
- `Flags` — flags (e.g. `HasLife`, `HasAgency`) used for visual styling; the canvas has no knowledge of what these mean to the magic system

The canvas is responsible for mapping world coordinates to screen coordinates. The game logic never knows or cares about screen size.

## Implementation notes

- ASP.NET Core Web API backend + static HTML/JS frontend.
- Terminal is xterm.js. Canvas is an inline SVG element.
- No real-time game loop — input is processed synchronously on Enter via `POST /command`.
- Canvas is SVG for the prototype (DOM hit-testing comes for free, sufficient for a small number of entities).

## Future: IDE functionality

Complex spells may eventually demand tooling beyond a raw REPL — things like a rune palette, expression tree visualisation, inline type hints, or a spell library browser. None of this is in scope for the first version. The terminal is sufficient to get spells working and observable.
