# Architecture — C4 Model

This section describes the architecture of the **RiveScript C#** library using the [C4 model](https://c4model.com/).
The C4 model provides four levels of abstraction: **Context**, **Container**, **Component**, and **Code**.

---

## Contents

| Level | Description |
|-------|-------------|
| [Level 1 — System Context](c4-context.md) | Who uses the library and what external things it interacts with |
| [Level 2 — Container](c4-container.md) | High-level building blocks of the system |
| [Level 3 — Component](c4-component.md) | Key components inside the library |
| [Level 4 — Code](c4-code.md) | Class-level design of the core components |

---

## Architecture Overview

RiveScript C# is a **single-assembly .NET library** (`RiveScript.dll`). It:

- Reads `.rive` / `.rs` script files (or streamed strings)
- Parses them into an in-memory Abstract Syntax Tree (AST)
- Sorts and indexes the triggers for efficient matching
- Responds to user messages by matching triggers and evaluating replies
- Manages per-user session data through a pluggable `ISessionManager`
- Allows embedding of C# code inside scripts via `IObjectHandler` / `ISubroutine`

The library is designed to be embedded directly into any .NET application —
desktop, web, mobile, or service — without any external dependencies.
