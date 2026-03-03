# API Reference

This section documents the complete public API of the **RiveScript C#** library.

---

## Primary Entry Point

| Class | Description |
|-------|-------------|
| [`RiveScriptEngine`](RiveScriptEngine.md) | Main interpreter class — load scripts, configure the engine, and get replies |
| [`Config`](Config.md) | Configuration options passed to the `RiveScriptEngine` constructor |
| [`ErrorMessages`](ErrorMessages.md) | Customise the error reply strings returned by the engine |

---

## Session Management

| Type | Description |
|------|-------------|
| [`ISessionManager`](ISessionManager.md) | Interface for user-session storage backends |
| [`ConcurrentDictionarySessionManager`](ConcurrentDictionarySessionManager.md) | Default thread-safe in-memory implementation |
| [`NoOpSessionManager`](NoOpSessionManager.md) | No-op implementation (stateless bots) |
| [`UserData`](UserData.md) | Per-user variable bag, history, and topic pointer |
| [`History`](History.md) | Rolling window of the last N input/reply pairs |
| [`ThawAction`](ThawAction.md) | Enumeration controlling what happens to a thawed session snapshot |

---

## Object Handlers & Macros

| Type | Description |
|------|-------------|
| [`IObjectHandler`](IObjectHandler.md) | Interface for language handlers that execute `> object` blocks |
| [`ISubroutine`](ISubroutine.md) | Minimal single-method interface for a named subroutine |
| [`DelegateMacro`](DelegateMacro.md) | Wraps a `Func<>` as an `ISubroutine` |

---

## AST (Abstract Syntax Tree)

| Class | Description |
|-------|-------------|
| [`Root`](Root.md) | Root of the parsed AST |
| [`Begin`](Begin.md) | Holds global `!define` data |
| [`Topic`](Topic.md) | Groups triggers by topic name |
| [`Trigger`](Trigger.md) | Single `+` pattern with its replies and conditions |
| [`ObjectMacro`](ObjectMacro.md) | Represents one `> object` block |

---

## Logging

| Type | Description |
|------|-------------|
| [`ILogger`](ILogger.md) | Four-level logging interface |
| [`ConsoleLogger`](ConsoleLogger.md) | Console-based logger |
| [`EmptyLogger`](EmptyLogger.md) | Silent (no-op) logger (default) |

---

## Enumerations & Constants

| Type | Description |
|------|-------------|
| [`ConcatMode`](ConcatMode.md) | Line continuation mode: `NONE`, `NEWLINE`, `SPACE` |
| [`Constants`](Constants.md) | Library-wide string constants |
