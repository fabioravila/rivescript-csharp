# C4 Level 2 — Container

The Container diagram zooms into the **RiveScript C# Library** and shows the high-level deployable / runnable units.

> For a library project a "container" is the assembly itself plus the external assets it works with.

```mermaid
C4Container
    title Container Diagram — RiveScript C# Library

    Person(developer, "Application Developer")

    System_Boundary(lib, "RiveScript C# Library (RiveScript.dll)") {
        Container(engine, "RiveScriptEngine", "C# Class", "Primary entry-point. Loads scripts, exposes reply() API, manages bot state")
        Container(parser, "Parser", "C# Class", "Lexes and parses RiveScript source into an AST")
        Container(topicMgr, "TopicManager", "C# Class", "Stores and indexes topics, triggers, and sort buffers")
        Container(sessionMgr, "SessionManager", "C# Interface + Impl", "Manages per-user variables, history, and topic state")
        Container(objectHandler, "ObjectHandler", "C# Interface + Impl", "Executes embedded code macros inside scripts")
        Container(sorting, "Sorting Engine", "C# Classes", "Sorts triggers by specificity for accurate matching")
    }

    System_Ext(scripts, "RiveScript Scripts (.rive/.rs)", "Plain-text conversation rules")
    System_Ext(externalStore, "External Session Store", "Optional: database, Redis, etc.")

    Rel(developer, engine, "Creates and configures", "new RiveScriptEngine(config)")
    Rel(developer, engine, "Calls", "loadFile() / stream() / reply()")
    Rel(engine, parser, "Delegates script loading to")
    Rel(parser, topicMgr, "Populates")
    Rel(engine, topicMgr, "Queries for matching triggers")
    Rel(engine, sessionMgr, "Reads/writes user session data")
    Rel(engine, objectHandler, "Invokes embedded code macros via")
    Rel(engine, sorting, "Calls sortReplies() on")
    Rel(parser, scripts, "Reads")
    Rel(sessionMgr, externalStore, "Optionally persists to", "custom ISessionManager impl")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Container Descriptions

| Container | Technology | Responsibility |
|---|---|---|
| **RiveScriptEngine** | C# class | Main public API. Orchestrates loading, sorting, and reply generation |
| **Parser** | C# class | Tokenises `.rive` source and builds the AST (`Root`, `Begin`, `Topic`, `Trigger`) |
| **TopicManager** | C# class | In-memory store of all topics and their triggers; produces sorted lookup tables |
| **SessionManager** | C# interface (`ISessionManager`) + default impl | Stores per-user variables, conversation history, and current topic |
| **ObjectHandler** | C# interface (`IObjectHandler`) + default C# impl | Executes code defined in `> object … < object` blocks |
| **Sorting Engine** | C# utility classes | Ranks triggers by word-count and specificity so the best match wins |

---

[← System Context](c4-context.md) | [Component Diagram →](c4-component.md)
