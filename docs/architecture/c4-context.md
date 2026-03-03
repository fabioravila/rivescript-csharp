# C4 Level 1 — System Context

The System Context diagram shows the **RiveScript C# library** in relation to its users and the external artefacts it depends on.

```mermaid
C4Context
    title System Context — RiveScript C# Interpreter

    Person(developer, "Application Developer", "Builds a .NET application that needs a chatbot")

    System(rivescript, "RiveScript C# Library", "Interprets RiveScript scripts and returns replies for user messages")

    System_Ext(scripts, "RiveScript Scripts (.rive/.rs)", "Plain-text script files that define the bot's conversation rules")
    System_Ext(hostApp, "Host Application", "Any .NET app: web API, desktop, console, service, etc.")
    System_Ext(sessionStore, "Session Storage", "Optional external storage for user session data (e.g., database, Redis)")

    Rel(developer, hostApp, "Builds and runs")
    Rel(hostApp, rivescript, "Uses", "calls loadFile/stream, reply()")
    Rel(developer, scripts, "Authors", "writes .rive files")
    Rel(rivescript, scripts, "Reads and parses")
    Rel(rivescript, sessionStore, "Reads / writes user vars", "via ISessionManager (optional)")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Description

| Actor / System | Role |
|---|---|
| **Application Developer** | Authors `.rive` script files and integrates the library into a .NET application |
| **RiveScript C# Library** | The core system — parses scripts, matches user input to triggers, and returns replies |
| **RiveScript Scripts** | Plain-text `.rive` / `.rs` files that define conversation rules |
| **Host Application** | Any .NET application (web API, console, desktop, background service) that embeds the library |
| **Session Storage** | Optional external store for user variables/history; defaults to an in-memory `ConcurrentDictionary` |

---

[← Architecture Overview](index.md) | [Container Diagram →](c4-container.md)
