using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiveScript
{
    public class Options
    {
        public bool debug { get; set; }
        public bool utf8 { get; set; }
        public bool strict { get; set; }
        public int depth { get; set; }
        public bool forceCase { get; set; }
        public ErrorMessages errors { get; set; }
        public Action<string> onDebug { get; set; }

        public static Options Default
        {
            get
            {
                return new Options
                {
                    debug = false,
                    depth = 50,
                    errors = null,
                    forceCase = false,
                    onDebug = null,
                    strict = true,
                    utf8 = false
                };
            }
        }
    }
}
