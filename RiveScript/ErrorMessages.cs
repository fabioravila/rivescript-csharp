using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiveScript
{
    public class ErrorMessages
    {
        public string replyNotMatched { get; set; }

        public string replyNotFound { get; set; }

        public string objectNotFound { get; set; }

        public string deepRecursion { get; set; }


        public static ErrorMessages Default
        {
            get
            {
                return new ErrorMessages
                {
                    deepRecursion = "ERR: Deep Recursion Detected",
                    objectNotFound = "[ERR: Object Not Found]",
                    replyNotFound = "ERR: No Reply Found",
                    replyNotMatched = "ERR: No Reply Matched"
                };
            }
        }


        public ErrorMessages AdjustDefaults()
        {
            var def = Default;

            deepRecursion = deepRecursion ?? def.deepRecursion;
            objectNotFound = objectNotFound ?? def.objectNotFound;
            replyNotFound = replyNotFound ?? def.replyNotFound;
            replyNotMatched = replyNotMatched ?? def.replyNotMatched;
            return this;
        }
    }
}
