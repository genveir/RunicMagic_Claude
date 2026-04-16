# Mud — Claude Workflow

## Testing

- Everything except the terminal/UI layer must be unit testable. Design all components accordingly — no integration tests should be necessary.
- All delivered code must be covered by tests. Tests must be run and passing before a ticket can move to Ready for Review.

## Communication

- Never use the `AskUserQuestion` widget. Ask questions as plain open-ended text instead.

## Architecture

- The solution follows a hub-and-spokes model. `Controller` is the hub and the only assembly that communicates between other assemblies. Spoke assemblies (`World`, `Database`, `Magic`, etc.) must not reference each other. If two spokes need to share a concept, it either lives in `Controller` or is intentionally represented differently in each spoke (e.g. `EntityData` uses `int TypeId` rather than `EntityType` to avoid `Database` depending on `World`).

## General

- Do not start implementing anything without explicit permission from the user.
  Discussing a ticket, explaining what it means, or asking questions about it is NOT permission to implement it.
  Wait for a clear instruction like "implement this" or "go ahead" before writing any code.
- At this stage of the project, the main risk is deferred decisions, not bad ones. Assist the user to make a call, document it in `Design/`, and move on. The codebase is small enough to swivel if something turns out wrong.

## Git

- The user handles all git operations: branching, committing, pushing, merging.
  Never run a Git command that would change the state of the repository, working directory, index, or any remote. If a command could alter, create, or delete any ref, file, object, or configuration, do not run it.

## Code Style

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

- Use named arguments when the purpose of an argument isn't obvious from the call site:
  - Always for inline lambdas (you can't tell from the lambda body alone which parameter it maps to)
  - Always for literals (bools, strings, numbers) where the meaning isn't self-evident from the method name
  - Not needed when passing a well-named variable (the variable name documents its purpose)
  - Not needed when the method name already makes the argument's role obvious (e.g. `RegisterInput("look")`, `SendOutput("hello")`)

## Design

- `Design/` contains settled decisions as per-area markdown files (e.g. `WorldModel.md`, `TypeSystem.md`).
- `Design/` is distinct from `Inspiration/`, which contains rough initial ideas and is not considered authoritative.
- The project is past the upfront design phase. New design documents are unlikely to be needed — from here on work is predominantly implementation. Do not propose writing design docs unless there is a genuine unresolved question that blocks building.
- Keep design docs lightweight — prose with headings, no formal structure required.

## Todo

- Work is tracked in `Todo.md` at the repo root.
- The To Do list is ordered by priority, top to bottom.
- Never move a ticket to "Done", move it to "Ready for Review" and the user will put it in "Done" when the user feels it is done.