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

- Use named arguments when the purpose of an argument isn't obvious from the call site:
  - Always for inline lambdas (you can't tell from the lambda body alone which parameter it maps to)
  - Always for literals (bools, strings, numbers) where the meaning isn't self-evident from the method name
  - Not needed when passing a well-named variable (the variable name documents its purpose)
  - Not needed when the method name already makes the argument's role obvious (e.g. `RegisterInput("look")`, `SendOutput("hello")`)

## Design

- Settled decisions are documented in `Design/` as per-area markdown files (e.g. `WorldModel.md`, `TypeSystem.md`).
- `Design/` is distinct from `Inspiration/`, which contains rough initial ideas and is not considered authoritative.
- When a ticket produces settled decisions, write them into the relevant `Design/` file before moving the ticket to Ready for Review.
- Keep design docs lightweight — prose with headings, no formal structure required.

## Todo

- Work is tracked in `Todo.md` at the repo root.
- The To Do list is ordered by priority, top to bottom.
- Never move a ticket to "Done", move it to "Ready for Review" and the user will put it in "Done" when the user feels it is done.