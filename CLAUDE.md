# Mud — Claude Workflow

## Testing

- Everything except the terminal/UI layer must be unit testable. Design all components accordingly — no integration tests should be necessary.
- All delivered code must be covered by tests. Tests must be run and passing before a ticket can move to Ready for Review.

## Communication

- Never use the `AskUserQuestion` widget. Ask questions as plain open-ended text instead.

## General

- Do not start implementing anything without explicit permission from the user.
  Discussing a ticket, explaining what it means, or asking questions about it is NOT permission to implement it.
  Wait for a clear instruction like "implement this" or "go ahead" before writing any code.

## Git

- The user handles all git operations: branching, committing, pushing, merging.
  Never run a Git command that would change the state of the repository, working directory, index, or any remote. If a command could alter, create, or delete any ref, file, object, or configuration, do not run it.

## Code Style

- Use named arguments when the purpose of an argument isn't obvious from the call site:
  - Always for inline lambdas (you can't tell from the lambda body alone which parameter it maps to)
  - Always for literals (bools, strings, numbers) where the meaning isn't self-evident from the method name
  - Not needed when passing a well-named variable (the variable name documents its purpose)
  - Not needed when the method name already makes the argument's role obvious (e.g. `RegisterInput("look")`, `SendOutput("hello")`)

## Todo

- Work is tracked in `Todo.md` at the repo root.
- Never move a ticket to "Done", move it to "Ready for Review" and the user will put it in "Done" when the user feels it is done.