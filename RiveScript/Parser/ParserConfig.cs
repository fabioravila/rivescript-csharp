namespace RiveScript.Parser
{
    public class ParserConfig
    {
        public bool strict { get; private set; }
        public bool utf8 { get; private set; }
        public bool forceCase { get; private set; }

        public ParserConfig(bool strict, bool utf8, bool forceCase)
        {
            this.strict = strict;
            this.utf8 = utf8;
            this.forceCase = forceCase;
        }

        public static ParserConfig Default => new ParserConfig(strict: false, utf8: false, forceCase: false);
    }
}
