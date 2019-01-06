using RiveScript.Lang;

namespace RiveScript
{
    public static class RiveScriptExtensions
    {
        public static void setCSharpHandler(this RiveScript rs)
        {
            rs.setHandler(Constants.CSharpHandlerName, new CSharp());
        }
    }
}
