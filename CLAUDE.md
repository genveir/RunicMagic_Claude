# RunicMagic — Claude Workflow

## User Profile

- The user is an expert-level C# developer. Frame all technical discussions at that level — no need to explain C# idioms or basic design patterns.
- The target audience for RunicMagic is senior software developers.

## Project Goals

- RunicMagic is a personal/hobbyist project — a fun sandbox and design prototype for an original TTRPG magic system. It will never be commercial.
- Favour interesting and correct design over defensive or scalable engineering. Be willing to explore bold ideas. Do not over-engineer for hypothetical scale.

## Implementation Permission - IMPORTANT
- NEVER start implementing ANYTHING without getting express permission from the user.
  Discussing a ticket, explaining what it means, or asking questions about it is NOT permission to implement it.
  Wait for a clear instruction like "implement this", "go ahead" before writing any code.

## Testing

- Everything except the terminal/UI layer must be unit testable. Design all components accordingly — no integration tests should be necessary.
- All delivered code must be covered by tests. Tests must be run and passing before a ticket can move to Ready for Review.

## Communication

- Never use the `AskUserQuestion` widget. Ask questions as plain open-ended text instead.

- Do not spawn Explore agents speculatively. Before reading any code, state what information is needed and why. If the user wants an Explore agent used, they will say so — and will specify what to look for.

- Act as a genuine sparring partner. When the user proposes something with a real downside, name it clearly and explain why — even if the user seems confident. Surface the tradeoff, then defer to their decision. Do not just agree to be agreeable.

## Architecture

- The solution follows a hub-and-spokes model. `Controller` is the hub and the only assembly that communicates between other assemblies. Spoke assemblies (`World`, `Database`, etc.) must not reference each other. If two spokes need to share a concept, it either lives in `Controller` or is intentionally represented differently in each spoke (e.g. `EntityData` uses `long TypeId` rather than `EntityType` to avoid `Database` depending on `World`).

## General

- At this stage of the project, the main risk is deferred decisions, not bad ones. Assist the user to make a call, document it in `Design/`, and move on. The codebase is small enough to swivel if something turns out wrong.

- Prefer working states over complete-but-unintegrated implementations. A minimal world model, minimal evaluation, and minimal UI that all run together is better than any one layer being fully built. When sequencing work, always look for the path to the next working state.

## Git

- The user handles all git operations: branching, committing, pushing, merging.
  Never run a Git command that would change the state of the repository, working directory, index, or any remote. If a command could alter, create, or delete any ref, file, object, or configuration, do not run it.

## Rune Formatting

- Always write rune references as `RUNEWORD(friendly name)` — e.g. `ZU(execute)`, `VUN(push)`, `A(me)`, `OH(this)`, `LA(scope of)`. Apply this in prose, code comments, design docs, and conversation.

## Code Style

- Use `long` (not `int`) for all domain numeric values. All measurements in the system are in millimetres and grams, making `int` overflow-prone. The only exceptions are:
  - Indices, counts, and other values required to be `int` by .NET or external APIs.
  - Geometry types (`Rectangle`, `Location`, `Direction`, and similar): use `double` there, because geometric computation involves trigonometry, sub-millimetre intermediate precision, and angles — floating-point arithmetic is correct by design.

- In the database schema, use `bigint` (not `int`) for all numeric columns under our control.

- SQL keywords are lowercase (`select`, `from`, `where`, `insert into`, etc.).

- Do not use column-aligned whitespace (extra spaces to align `=`, `=>`, `:`, or property values into columns). The project uses an auto-formatter on save that strips this, so it creates noise in diffs.

- Object initializers with multiple properties use one property per line — do not pack multiple assignments onto a single line.

- Do not use expression-bodied methods (`=> expression`). Always use block bodies, assign the result to a named local variable, and return that variable. This makes every intermediate value visible in the debugger's Locals/Watch windows:
  ```csharp
  // wrong
  public Entity? Find(EntityId id) => _entities.GetValueOrDefault(id);

  // right
  public Entity? Find(EntityId id)
  {
      var entity = _entities.GetValueOrDefault(id);
      return entity;
  }
  ```
  Void methods that have nothing to return are exempt.

- Do not use primary constructors except in records. Always use an explicit constructor body so fields are declared separately and remain navigable and debuggable.

- Use named arguments when the purpose of an argument isn't obvious from the call site:
  - Always for inline lambdas (you can't tell from the lambda body alone which parameter it maps to)
  - Always for literals (bools, strings, numbers) where the meaning isn't self-evident from the method name
  - Always when a method has multiple parameters of the same type — the compiler won't catch argument order errors, so the call site must make the mapping explicit
  - Not needed when the method name already makes the argument's role obvious (e.g. `RegisterInput("look")`, `SendOutput("hello")`)

## Design

- Authoritative design documents live in `Design/` as per-area markdown files. Always `glob Design/` first rather than hardcoding the list. Read them directly when working on anything they cover — never rely on a memory summary, as they evolve.
- Keep design docs lightweight — prose with headings, no formal structure required.

## Todo

- Work is tracked in `Todo.md` at the repo root.
- **To Do — Milestone N** contains the focused set of tickets on the critical path to the next milestone flag. These are the priority.
- **To Do — Other** contains valid tickets that are out of scope for the current milestone.
- When starting work on a ticket — including design work — move it to **In Progress**.
- When implementation is complete and tests are passing, move it to **Ready for Review**. Never move it to "Done"; the user does that.
