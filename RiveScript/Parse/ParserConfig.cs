namespace RiveScript.Parse
{
    /// <summary>
    /// User-configurable properties of the RiveScript
    /// </summary>
    public class ParserConfig
    {
        public bool strict { get; private set; }
        public bool utf8 { get; private set; }
        public bool forceCase { get; private set; }
        public ILogger logger { get; private set; }
        public ConcatMode concat { get; private set; }

        public ParserConfig(bool strict, bool utf8, bool forceCase, ConcatMode concat, ILogger logger)
        {
            this.strict = strict;
            this.utf8 = utf8;
            this.forceCase = forceCase;
            this.logger = logger;
            this.concat = concat;
        }

        public static ParserConfig Default => new ParserConfig(strict: false, utf8: false, forceCase: false, concat: ConcatMode.NONE, logger: null);
    }
}
