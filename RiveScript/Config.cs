using RiveScript.Session;
using System;


namespace RiveScript
{
    public class Config
    {
        public const int DEFAULT_DEPTH = 50;
        public static readonly string DEFAULT_UNICODE_PUNCTUATION_PATTERN = "[., !?;:]";


        public bool debug { get; set; }
        public bool utf8 { get; set; }
        public bool strict { get; set; }
        public bool throwExceptions { get; set; }
        public string unicodePonctuations { get; set; }
        public int depth { get; set; }
        public bool forceCase { get; set; }
        public ErrorMessages errors { get; set; }
        public Action<string> onDebug { get; set; }
        public ILogger logger { get; set; }
        public ISessionManager sessionManager { get; set; }

        public static Config Default
        {
            get
            {
                return new Config
                {
                    debug = false,
                    depth = DEFAULT_DEPTH,
                    errors = null,
                    unicodePonctuations = DEFAULT_UNICODE_PUNCTUATION_PATTERN,
                    throwExceptions = false,
                    forceCase = false,
                    onDebug = null,
                    strict = true,
                    utf8 = false,
                    logger = null,
                    sessionManager = null
                };
            }
        }

        public static Config UTF8
        {
            get
            {
                var def = Default;
                def.utf8 = true;
                return def;
            }
        }

        public static Config Debug
        {
            get
            {
                var def = Default;
                def.debug = true;
                return def;
            }
        }

        public static Config DebugUTF8
        {
            get
            {
                var def = Default;
                def.debug = true;
                def.utf8 = true;
                return def;
            }
        }
    }
}
