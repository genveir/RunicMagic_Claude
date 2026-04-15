# UI

## Layout

The UI is split into two panels:

- **Canvas (top)** — a 2D view of the world. Entities are rendered as labelled rectangles, directly mirroring the world model's axis-aligned rectangle representation. Spatial relationships (containment, touch, proximity) are visible at a glance.
- **Terminal (bottom)** — a REPL for spell input and world event output. The player types rune strings and hits Enter; results and world events are echoed back as text.

The canvas redraws after each command, in the same render pass as the terminal output flush.

## Player interface

The session interface (`IPlayerViewInterface`) has two output channels:

- **Text channel** — `onTextDataAvailable(string)` callback, called whenever the backend has text to render in the terminal. Used for spell results and world event messages.
- **Canvas rendering data channel** — `onRenderingDataAvailable(EntityRenderingModel)` callback, called for each entity to draw this tick; the canvas replaces its entity list and redraws.

Both outputs flush on `onFlush()`, output everything in their buffer and clear it.

`EntityRenderingModel` carries everything the canvas needs and nothing else:

- `X`, `Y`, `Width`, `Height` — position and dimensions in world coordinates
- `Label` — display name
- `Flags` — flags (e.g. `HasLife`, `HasAgency`) used for visual styling; the canvas has no knowledge of what these mean to the magic system

The canvas is responsible for mapping world coordinates to screen coordinates. The game logic never knows or cares about screen size.

## Implementation notes

- Blazor Server + xterm.js for the terminal, similar to the Mud project (`E:\repos\Mud\Terminal.md`).
- No real-time game loop — input is processed synchronously on Enter.
- Canvas is SVG for the prototype (DOM hit-testing comes for free, sufficient for a small number of entities).

## Future: IDE functionality

Complex spells may eventually demand tooling beyond a raw REPL — things like a rune palette, expression tree visualisation, inline type hints, or a spell library browser. None of this is in scope for the first version. The terminal is sufficient to get spells working and observable.
