# C4 Level 4 — Code

The Code (class) diagram shows the key classes, their public members, and relationships inside the **RiveScript C# library**.

> Only the most important public members are shown for clarity.

---

## Core Classes

```mermaid
classDiagram
    class RiveScriptEngine {
        +bool IsErrReply(string reply)
        +string getVersion()
        +bool loadFile(string file)
        +bool loadDirectory(string path)
        +bool loadDirectory(string path, string[] exts)
        +bool stream(string code)
        +bool stream(string[] code)
        +void sortReplies()
        +string reply(string username, string message)
        +bool setGlobal(string name, string value)
        +string getGlobal(string name)
        +bool setVariable(string name, string value)
        +string getVariable(string name)
        +bool setUservar(string user, string name, string value)
        +string getUserVar(string user, string name)
        +bool setSubroutine(string name, ISubroutine sub)
        +void setHandler(string name, IObjectHandler handler)
        +ISessionManager getSessionManager()
        +string currentUser()
        +ErrorMessages errors
    }

    class Config {
        +bool debug
        +bool utf8
        +bool strict
        +bool throwExceptions
        +bool forceCase
        +int depth
        +ConcatMode concat
        +ILogger logger
        +ISessionManager sessionManager
        +ErrorMessages errors
        +Action~string~ onDebug
        +string unicodePonctuations
        +$Config Default
        +$Config UTF8
        +$Config Debug
        +$Config DebugUTF8
    }

    class ErrorMessages {
        +string deepRecursion
        +string replyNotMatched
        +string replyNotFound
        +string objectNotFound
        +$ErrorMessages Default
        +ErrorMessages AdjustDefaults()
    }

    RiveScriptEngine --> Config : configured by
    RiveScriptEngine --> ErrorMessages : uses
```

---

## Parser & AST

```mermaid
classDiagram
    class Parser {
        +Parser(TopicManager topicManager)
        +Parser(ParserConfig config, TopicManager topicManager)
        +Root parse(string filename, string[] code)
    }

    class ParserConfig {
        +bool strict
        +bool utf8
        +bool forceCase
        +ConcatMode concat
        +ILogger logger
        +$ParserConfig Default
    }

    class Root {
        +Begin begin
        +List~ObjectMacro~ objects
        +IDictionary~string,Topic~ topics
        +void addTopic(Topic topic)
        +void addObject(ObjectMacro obj)
    }

    class Begin {
        +IDictionary~string,string~ globals
        +IDictionary~string,string~ vars
        +IDictionary~string,string~ subs
        +IDictionary~string,string~ person
        +IDictionary~string,ICollection~string~~ arrays
    }

    class Topic {
        +string name
        +bool hasPrevious
        +ICollection~string~ includes
        +ICollection~string~ inherits
        +Trigger getTrigger(string pattern)
        +void addIncludes(string topic)
        +void addInherits(string topic)
        +void sortTriggers()
        +string[] getTopics()
    }

    class Trigger {
        +string pattern
        +ICollection~string~ reply
        +ICollection~string~ condition
        +string redirect
        +bool hasOwnCode
        +IDictionary~string,ICollection~string~~ previous
    }

    class ObjectMacro {
        +string Name
        +string Language
        +List~string~ Code
    }

    Parser --> ParserConfig : uses
    Parser ..> Root : produces
    Root *-- Begin : contains
    Root *-- "0..*" Topic : contains
    Root *-- "0..*" ObjectMacro : contains
    Topic *-- "0..*" Trigger : contains
```

---

## Topic Manager & Sorting

```mermaid
classDiagram
    class TopicManager {
        +Topic getTopic(string name)
        +bool exists(string name)
        +string[] listTopicsName()
        +IDictionary~string,Topic~ listTopics()
        +void sortReplies()
    }

    class SortBuffer {
        +string[] topics
        +IDictionary~string,string[]~ thats
    }

    class SortTrack {
        +int wildcards
        +int optionals
        +int namedWildcards
        +int wordCount
    }

    class SortedTriggerEntry {
        +string trigger
        +Trigger pointer
    }

    TopicManager --> "0..*" Topic : manages
    TopicManager --> SortBuffer : builds
    SortBuffer --> "0..*" SortedTriggerEntry : contains
    SortTrack ..> SortedTriggerEntry : helps rank
```

---

## Session Management

```mermaid
classDiagram
    class ISessionManager {
        <<interface>>
        +UserData init(string username)
        +void set(string username, string name, string value)
        +void set(string username, IDictionary~string,string~ vars)
        +void addHistory(string username, string input, string reply)
        +void setLastMatch(string username, string trigger)
        +string get(string username, string name)
        +UserData get(string username)
        +IDictionary~string,UserData~ getAll()
        +string getLastMatch(string username)
        +History getHistory(string username)
        +void clear(string username)
        +void clearAll()
        +void freeze(string username)
        +void thaw(string username, ThawAction action)
        +void remove(string username, string name)
    }

    class ConcurrentDictionarySessionManager {
        +UserData init(string username)
        +void set(...)
        +UserData get(string username)
        +void freeze(string username)
        +void thaw(string username, ThawAction action)
    }

    class NoOpSessionManager {
        +UserData init(string username)
        +void set(...)
    }

    class UserData {
        +string username
        +string lastMatch
        +History history
        +void setVariable(string name, string value)
        +string getVariable(string name)
        +bool removeVariable(string name)
        +IDictionary~string,string~ getVariables()
        +History getHistory()
    }

    class History {
        +IList~string~ input
        +IList~string~ reply
        +void add(string input, string reply)
    }

    class ThawAction {
        <<enumeration>>
        KEEP
        THAW
        DISCARD
    }

    ISessionManager <|.. ConcurrentDictionarySessionManager : implements
    ISessionManager <|.. NoOpSessionManager : implements
    ISessionManager ..> UserData : returns
    UserData *-- History : contains
    ISessionManager ..> ThawAction : uses
```

---

## Object Handlers & Macros

```mermaid
classDiagram
    class IObjectHandler {
        <<interface>>
        +void Load(string name, string[] code)
        +string Call(string name, RiveScriptEngine rs, string[] args)
    }

    class CSharpObjectHandler {
        +void Load(string name, string[] code)
        +string Call(string name, RiveScriptEngine rs, string[] args)
        +void AddSubroutine(string name, ISubroutine sub)
    }

    class ISubroutine {
        <<interface>>
        +string Call(RiveScriptEngine rs, string[] args)
    }

    class DelegateMacro {
        +DelegateMacro(Func~RiveScriptEngine,string[],string~ func)
        +string Call(RiveScriptEngine rs, string[] args)
    }

    IObjectHandler <|.. CSharpObjectHandler : implements
    CSharpObjectHandler --> "0..*" ISubroutine : hosts named
    ISubroutine <|.. DelegateMacro : implements
```

---

## Logging

```mermaid
classDiagram
    class ILogger {
        <<interface>>
        +bool IsDebugEnable
        +bool IsWarnEnable
        +bool IsTraceEnable
        +bool IsErrorEnable
        +void Debug(string text)
        +void Warn(string text)
        +void Warn(string text, string filename, int lineno)
        +void Error(string text)
        +void Error(Exception exception)
        +void Trace(string text)
    }

    class ConsoleLogger {
        +void Debug(string text)
        +void Warn(string text)
        +void Error(string text)
        +void Trace(string text)
    }

    class EmptyLogger {
        +void Debug(string text)
        +void Warn(string text)
        +void Error(string text)
        +void Trace(string text)
    }

    ILogger <|.. ConsoleLogger : implements
    ILogger <|.. EmptyLogger : implements
```

---

[← Component Diagram](c4-component.md) | [Architecture Overview ↑](index.md)
