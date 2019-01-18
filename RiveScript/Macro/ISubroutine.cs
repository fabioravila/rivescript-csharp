namespace RiveScript.Macro
{
    /// <summary>
    /// Interface for RiveScript object macros in CSharp .NET 
    /// </summary>
    public interface ISubroutine
    {
        string Call(RiveScriptEngine rs, string[] args);
    }
}
