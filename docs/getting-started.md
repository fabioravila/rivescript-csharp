# Getting Started

This guide walks you through installing the library and building your first chatbot.

---

## Installation

### Via NuGet (recommended)

```bash
dotnet add package RiveScript
```

Or in the NuGet Package Manager Console:

```powershell
Install-Package RiveScript
```

### From Source

```bash
git clone https://github.com/fabioravila/rivescript-csharp.git
cd rivescript-csharp
# Build with Visual Studio or MSBuild
msbuild RiveScript.sln /p:Configuration=Release
```

---

## Creating Your First Bot

### 1 — Create a RiveScript Engine

```csharp
using RiveScript;

// Default engine (strict mode, no debug, UTF-8 disabled)
var rs = new RiveScriptEngine();

// Or with custom options
var rs = new RiveScriptEngine(new Config
{
    debug  = false,
    utf8   = true,
    strict = true,
    depth  = 50
});
```

### 2 — Load RiveScript Content

There are three ways to load script content into the engine:

**a) Load a single file**

```csharp
rs.loadFile("brain/greetings.rive");
```

**b) Load an entire directory**

```csharp
rs.loadDirectory("brain/");            // loads all .rive and .rs files
rs.loadDirectory("brain/", new[] { ".rive" }); // custom extension filter
```

**c) Stream a string directly (great for tests)**

```csharp
rs.stream(@"
    + hello
    - Hi there!

    + goodbye
    - Bye bye!
");
```

### 3 — Sort Replies

After loading all scripts, call `sortReplies()` **once** before the first `reply()` call.
This builds the internal sort buffers for accurate trigger matching.

```csharp
rs.sortReplies();
```

### 4 — Get a Reply

```csharp
string answer = rs.reply("user123", "hello");
Console.WriteLine(answer); // Hi there!
```

---

## RiveScript Syntax Cheat-Sheet

| Symbol | Meaning | Example |
|--------|---------|---------|
| `!`    | Define  | `! var name = Aiden` |
| `+`    | Trigger | `+ hello bot` |
| `-`    | Reply   | `- Hello, human!` |
| `^`    | Continue (multi-line reply) | `^ More text here` |
| `@`    | Redirect | `@ greetings` |
| `%`    | Previous | `% what is your name` |
| `*`    | Condition | `* <get age> > 18 => You are an adult.` |
| `>`    | Label (topic/object) | `> topic mytopic` |
| `<`    | End label | `< topic` |

---

## Configuration Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `debug` | `bool` | `false` | Enable debug logging |
| `strict` | `bool` | `true` | Strict parse mode — throw on invalid syntax |
| `utf8` | `bool` | `false` | Enable UTF-8 mode (allows Unicode in triggers) |
| `depth` | `int` | `50` | Maximum recursion depth for reply redirections |
| `forceCase` | `bool` | `false` | Force lowercase on all triggers and user input |
| `concat` | `ConcatMode` | `NONE` | Line continuation mode (`NONE`, `NEWLINE`, `SPACE`) |
| `throwExceptions` | `bool` | `false` | Throw `RiveScriptException` instead of returning error strings |
| `logger` | `ILogger` | `EmptyLogger` | Inject a custom logger |
| `sessionManager` | `ISessionManager` | `ConcurrentDictionarySessionManager` | Custom session storage |
| `errors` | `ErrorMessages` | `ErrorMessages.Default` | Customise error reply strings |

---

## Topics

Topics let you group triggers and create context-aware conversations:

```rivescript
> topic greetings

    + hello
    - Hi! How can I help you today?

    + bye
    - Goodbye! {topic=random}

< topic
```

Switch a user to a topic programmatically:

```csharp
rs.setUservar("user123", "topic", "greetings");
```

---

## Object Macros (Embedded C#)

You can embed C# code directly in `.rive` scripts:

```rivescript
> object add csharp
    int a = int.Parse(args[0]);
    int b = int.Parse(args[1]);
    return (a + b).ToString();
< object

+ add [*] and [*]
- The sum is: <call>add <star1> <star2></call>
```

Or register a subroutine from C#:

```csharp
rs.setSubroutine("greet", new DelegateMacro((engine, args) =>
{
    return $"Hello, {args[0]}!";
}));
```

---

## Custom Session Manager

Implement `ISessionManager` to store user data in a database, Redis, or any other backend:

```csharp
public class MySessionManager : ISessionManager
{
    // Implement all interface members ...
}

var rs = new RiveScriptEngine(new Config
{
    sessionManager = new MySessionManager()
});
```

---

## Error Handling

By default the engine returns error strings (e.g. `"ERR: Deep Recursion"`) instead of throwing.
You can check for errors with:

```csharp
if (rs.IsErrReply(reply))
{
    // handle error
}
```

Or enable exception throwing:

```csharp
var rs = new RiveScriptEngine(new Config { throwExceptions = true });
```

---

## Next Steps

- [API Reference](api/index.md) — full API documentation
- [Architecture](architecture/index.md) — C4 diagrams and design overview
- [RiveScript Community Wiki](https://github.com/aichaos/rivescript/wiki)
