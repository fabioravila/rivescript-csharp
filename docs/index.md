# RiveScript C# — Documentation

[![Build status](https://ci.appveyor.com/api/projects/status/crwboe7fa6aseqvc/branch/master?svg=true)](https://ci.appveyor.com/project/fabioravila/rivescript-csharp/branch/master)
[![MIT License](https://img.shields.io/github/license/fabioravila/rivescript-csharp.svg)](../LICENSE)

## What is RiveScript C#?

**RiveScript C#** is a C# implementation of the [RiveScript](http://www.rivescript.com/) scripting language interpreter. RiveScript is a simple scripting language for authoring chatbots and gives you a simple, yet powerful, way to define conversational flows.

This library implements the [RiveScript 2.0 Working Draft](http://www.rivescript.com/wd/RiveScript.html) and was initially developed based on the official [rivescript-java](https://github.com/aichaos/rivescript-java) interpreter.

---

## Contents

| Section | Description |
|---|---|
| [Getting Started](getting-started.md) | Install and create your first bot in minutes |
| [API Reference](api/index.md) | Full public API reference for `RiveScriptEngine` and related types |
| [Architecture](architecture/index.md) | C4 architecture diagrams and design overview |

---

## Quick Example

```csharp
using RiveScript;

// Create an engine instance
var rs = new RiveScriptEngine();

// Load script from a string
rs.stream(@"
    + hello bot
    - Hello, human!

    + what is your name
    - My name is Aiden.
");

// Sort replies (required before first call to reply())
rs.sortReplies();

// Get a reply
string answer = rs.reply("localuser", "hello bot");
Console.WriteLine(answer); // → Hello, human!
```

---

## Key Features

- **RiveScript 2.0** compliant interpreter
- **UTF-8** support for multilingual bots
- **Topic inheritance & includes** for modular scripts
- **Wildcards & alternations** in triggers
- **Conditionals** (`*condition - reply`)
- **Substitutions** (input and person substitutions)
- **Object macros** — embed C# code inside `.rive` scripts
- **Custom session managers** (in-memory by default; implement `ISessionManager` for custom storage)
- **Thread-safe** design using `ThreadLocal` user context
- **Configurable** via the `Config` class

---

## Community & Support

- [RiveScript Community Wiki](https://github.com/aichaos/rivescript/wiki)
- [GitHub Issues](https://github.com/fabioravila/rivescript-csharp/issues)
