# C4 Level 3 — Component

The Component diagram zooms into the **RiveScriptEngine** and the supporting subsystems to show the key classes and their responsibilities.

```mermaid
C4Component
    title Component Diagram — RiveScript C# Library (RiveScript.dll)

    Container_Boundary(engine_b, "RiveScriptEngine") {
        Component(rse, "RiveScriptEngine", "Public API Class", "Exposes loadFile(), stream(), sortReplies(), reply(), setUservar(), etc.")
        Component(config, "Config", "POCO", "Holds all startup options: debug, utf8, strict, depth, sessionManager, logger, etc.")
        Component(errorMessages, "ErrorMessages", "POCO", "Customisable reply strings for deep-recursion, not-found, etc.")
    }

    Container_Boundary(parser_b, "Parser Subsystem") {
        Component(parser, "Parser", "Class", "Tokenises RiveScript source line by line and produces an AST Root object")
        Component(parserConfig, "ParserConfig", "POCO", "strict, utf8, forceCase, concat settings for the parser")
        Component(ast_root, "Root (AST)", "Class", "Root of the parsed AST — holds Begin data and ObjectMacros")
        Component(ast_begin, "Begin (AST)", "Class", "Holds !global, !var, !sub, !person, !array definitions")
        Component(ast_topic, "Topic (AST)", "Class", "Holds all triggers for one topic")
        Component(ast_trigger, "Trigger (AST)", "Class", "One + trigger with its replies, conditions, redirects, and %Previous")
        Component(ast_macro, "ObjectMacro (AST)", "Class", "Holds code for an embedded > object ... < object block")
    }

    Container_Boundary(topic_b, "Topic Subsystem") {
        Component(topicMgr, "TopicManager", "Class", "Registry of all Topic instances; orchestrates inheritance / includes resolution")
        Component(sortBuffer, "SortBuffer", "Class", "Per-topic sorted trigger array produced by sortReplies()")
        Component(sortTrack, "SortTrack", "Class", "Tracks sort weight for each trigger during sort phase")
    }

    Container_Boundary(session_b, "Session Subsystem") {
        Component(iSession, "ISessionManager", "Interface", "Contract for user session storage")
        Component(concSession, "ConcurrentDictionarySessionManager", "Default Impl", "Thread-safe in-memory session store")
        Component(noopSession, "NoOpSessionManager", "Impl", "No-op implementation (useful for stateless bots)")
        Component(userData, "UserData", "Class", "Per-user bag of variables, history, frozen snapshot, and current topic")
        Component(history, "History", "Class", "Rolling window of last N inputs and replies for a user")
    }

    Container_Boundary(macro_b, "Object Handler Subsystem") {
        Component(iHandler, "IObjectHandler", "Interface", "Contract for loading and calling embedded code objects")
        Component(csharpHandler, "CSharpObjectHandler", "Default Impl", "Compiles and executes C# snippets embedded in .rive scripts")
        Component(iSubroutine, "ISubroutine", "Interface", "Single-method contract for a named subroutine callable from scripts")
        Component(delegateMacro, "DelegateMacro", "Impl", "Wraps a C# Func<> as an ISubroutine")
    }

    Container_Boundary(log_b, "Logging") {
        Component(iLogger, "ILogger", "Interface", "Debug/Info/Warn/Error logging contract")
        Component(consoleLogger, "ConsoleLogger", "Impl", "Writes to Console.WriteLine")
        Component(emptyLogger, "EmptyLogger", "Impl", "Discards all log calls (default)")
    }

    Rel(rse, config, "Configured by")
    Rel(rse, parser, "Delegates script loading to")
    Rel(rse, topicMgr, "Queries trigger matches from")
    Rel(rse, iSession, "Reads/writes user data via")
    Rel(rse, iHandler, "Calls macro code via")
    Rel(rse, iLogger, "Logs via")
    Rel(parser, parserConfig, "Uses settings from")
    Rel(parser, ast_root, "Produces")
    Rel(ast_root, ast_begin, "Contains")
    Rel(ast_root, ast_topic, "Contains many")
    Rel(ast_root, ast_macro, "Contains many")
    Rel(ast_topic, ast_trigger, "Contains many")
    Rel(topicMgr, ast_topic, "Manages")
    Rel(topicMgr, sortBuffer, "Builds and caches")
    Rel(sortBuffer, sortTrack, "Uses during sort")
    Rel(iSession, userData, "Returns")
    Rel(userData, history, "Contains")
    Rel(concSession, iSession, "Implements")
    Rel(noopSession, iSession, "Implements")
    Rel(csharpHandler, iHandler, "Implements")
    Rel(delegateMacro, iSubroutine, "Implements")
    Rel(csharpHandler, iSubroutine, "Hosts named")
    Rel(consoleLogger, iLogger, "Implements")
    Rel(emptyLogger, iLogger, "Implements")
```

## Component Descriptions

### RiveScriptEngine Boundary

| Component | Description |
|---|---|
| `RiveScriptEngine` | The sole public entry-point. Coordinates all subsystems. |
| `Config` | Immutable (or default) options passed at construction time. |
| `ErrorMessages` | Override the reply strings returned on known error conditions. |

### Parser Subsystem

| Component | Description |
|---|---|
| `Parser` | Reads source lines, recognises command symbols (`+`, `-`, `^`, `@`, `*`, `>`, `<`, `!`, `%`), and builds the AST. |
| `ParserConfig` | Carries parser-specific settings derived from the top-level `Config`. |
| `Root` | Tree root; holds the global `Begin` node, all `Topic` nodes, and `ObjectMacro` nodes. |
| `Begin` | Stores `! global`, `! var`, `! sub`, `! person`, and `! array` definitions. |
| `Topic (AST)` | An AST node that groups triggers by topic name. |
| `Trigger` | An AST node for one `+` pattern together with its replies, conditions, and redirects. |
| `ObjectMacro` | An AST node for one `> object` block, carrying its language name and code lines. |

### Topic Subsystem

| Component | Description |
|---|---|
| `TopicManager` | Central registry. Resolves `includes` and `inherits` links between topics and drives sorting. |
| `SortBuffer` | Holds the sorted trigger array for one topic after `sortReplies()` is called. |
| `SortTrack` | Accumulates weighting metrics (word count, wildcards) for each trigger during the sort pass. |

### Session Subsystem

| Component | Description |
|---|---|
| `ISessionManager` | Plug-in interface for user-data storage. |
| `ConcurrentDictionarySessionManager` | Default, thread-safe, in-memory implementation. |
| `NoOpSessionManager` | Ignores all writes; useful for stateless or read-only scenarios. |
| `UserData` | Holds a user's variables map, current topic, history, and frozen snapshot. |
| `History` | Rolling buffer of the last N (default 9) input/reply pairs for a user. |

### Object Handler Subsystem

| Component | Description |
|---|---|
| `IObjectHandler` | Interface that all language handlers must implement (`Load`, `Call`). |
| `CSharpObjectHandler` | Default handler; compiles and caches C# macro code with Roslyn/CodeDOM. |
| `ISubroutine` | Minimal interface for a single named subroutine. |
| `DelegateMacro` | Bridges a `Func<RiveScriptEngine, string[], string>` to `ISubroutine`. |

### Logging

| Component | Description |
|---|---|
| `ILogger` | Four-level (`Debug`, `Info`, `Warn`, `Error`) logging interface. |
| `ConsoleLogger` | Writes all log events to `Console.WriteLine`. |
| `EmptyLogger` | Default; silently discards all log events. |

---

[← Container Diagram](c4-container.md) | [Code Diagram →](c4-code.md)
