namespace RiveScript.Session
{
    /// <summary>
    /// Specifies options for thawing the user's variables
    /// </summary>
    public enum ThawAction
    {
        /// <summary>
        /// Restore the variables and delete the frozen copy.
        /// </summary>
        THAW = 1,

        /// <summary>
        /// Don't restore the variables, just delete the frozen copy.
        /// </summary>
        DISCARD,

        /// <summary>
        ///  Keep the frozen copy after restoring.
        /// </summary>
        KEEP
    }
}
