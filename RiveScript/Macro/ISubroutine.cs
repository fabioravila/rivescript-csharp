namespace RiveScript.Macro
{
    /// <summary>
    /// Interface for RiveScript object macros in CSharp .NET 
    /// </summary>
    public interface ISubroutine
    {
        string Call(RiveScript rs, string[] args);
    }
}
