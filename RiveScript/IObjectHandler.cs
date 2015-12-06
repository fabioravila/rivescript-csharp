
namespace RiveScript
{
    public interface IObjectHandler
    {
        /// <summary>
        /// Handler for when object code is read (loaded) by RiveScript.
        /// Should return true for success or false to indicate error.
        /// </summary>
        /// <param name="name">The name of the object</param>
        /// <param name="code">The source code inside the object</param>
        /// <returns></returns>
        bool onLoad(string name, string[] code);

        /// <summary>
        /// Handler for when a user invokes the object. Should return the text
        /// reply from the object.
        /// </summary>
        /// <param name="name">The name of the object being called</param>
        /// <param name="user">The user's ID</param>
        /// <param name="args">The argument list from the call tag</param>
        /// <returns></returns>
        string onCall(string name, RiveScript rs, string[] args);
    }
}
