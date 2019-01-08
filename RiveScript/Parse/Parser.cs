using RiveScript.AST;
using RiveScript.Exceptions;
using RiveScript.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RiveScript.Parse
{
    /// <summary>
    /// Parser for RiveScript source code
    /// </summary>
    public class Parser
    {
        // Constant RiveScript command symbols.
        const double RS_VERSION = 2.0; // This implements RiveScript 2.0
        const string CMD_DEFINE = "!";
        const string CMD_TRIGGER = "+";
        const string CMD_PREVIOUS = "%";
        const string CMD_REPLY = "-";
        const string CMD_CONTINUE = "^";
        const string CMD_REDIRECT = "@";
        const string CMD_CONDITION = "*";
        const string CMD_LABEL = ">";
        const string CMD_ENDLABEL = "<";
        static IDictionary<string, string> concatModes;


        static Parser()
        {
            concatModes = new Dictionary<string, string>();
            concatModes.Add("none", "");
            concatModes.Add("newline", "\n");
            concatModes.Add("space", " ");
        }

        bool strict;
        bool utf8;
        bool forceCase;
        ILogger logger = new EmptyLogger(); //TODO adjust

        public Parser() : this(null) { }

        public Parser(ParserConfig config)
        {
            if (config == null)
                config = ParserConfig.Default;

            strict = config.strict;
            utf8 = config.utf8;
            forceCase = config.forceCase;
        }


        private Root parse(string filename, string[] code)
        {
            logger.debug($"Parsing {filename}");
            if (logger.isTraceEnable)
            {
                foreach (var line in code)
                {
                    logger.trace(line);
                }
            }




            var startTime = DateTime.Now.Ticks;
            var ast = new Root();


            // Track some state variables for this parsing round.
            string topic = "random";                    // Default topic = random
            int lineno = 0;
            bool inComment = false;                     // In a multi-line comment
            bool inObject = false;                      // In an object
            string objectName = null;                   // Name of the current object
            string objectLanguage = null;               // Programming language of the object
            ICollection<string> objectBuffer = null;    // Buffer for the current object
            string currentTrigger = null;               // Trigger we're on
            Trigger currentTriggerObject = null;        // Trigger we're on
            string previous = null;                     // The a %Previous trigger

            // File scoped parser options.
            IDictionary<string, string> local_options = new Dictionary<string, string>();
            local_options.AddOrUpdate("concat", "none");
            //local_options.Add("concat", "space");
            //local_options.Add("concat", "newline");

            // The given "code" is an array of lines, so jump right in.
            for (int i = 0; i < code.Length; i++)
            {
                lineno++; // Increment the line counter.
                string line = code[i];
                logger.debug("Original Line: " + line);



                // Trim the line of whitespaces.
                line = Util.Strip(line);

                if (line.Length == 0)
                    continue; //Skip blank line

                // Are we inside an object?
                if (inObject)
                {
                    if (line.StartsWith("<object") || line.StartsWith("< object"))
                    {
                        // End of the object. Did we have a handler?
                        if (!string.IsNullOrWhiteSpace(objectName))
                        {
                            var macro = new ObjectMacro
                            {
                                code = objectBuffer,
                                Language = objectLanguage,
                                Name = objectName,
                            };

                            ast.addObject(macro);

                        }
                        objectName = null;
                        objectLanguage = null;
                        objectBuffer = null;
                        inObject = false;
                    }
                    else
                    {
                        // Collect the code.
                        objectBuffer.Add(line);
                    }

                    continue;
                }

                // Look for comments.
                if (line.StartsWith("/*"))
                {
                    // Beginning a multi-line comment.
                    if (line.IndexOf("*/") > -1)
                    {
                        // It ends on the same line.
                        continue;
                    }
                    inComment = true;
                }
                else if (line.StartsWith("//"))
                {
                    // A single line comment.
                    continue;
                }
                else if (line.IndexOf("*/") > -1)
                {
                    // End a multi-line comment.
                    inComment = false;
                    continue;
                }
                if (inComment) //else if?
                {
                    continue;
                }

                // Skip any blank lines and weird lines.
                if (line.Length < 2)
                {
                    logger.warn($"Weird single-character line '#" + line + "' found (in topic #" + topic + ")");
                    continue;
                }

                // Separate the command from the rest of the line.
                string cmd = line.Substring(0, 1);
                line = Util.Strip(line.Substring(1));


                logger.debug("\tCmd: " + cmd);

                // Ignore in-line comments if there's a space before and after the "//".
                if (line.IndexOf(" // ") > -1)
                {
                    string[] split = line.Split(new[] { " // " }, StringSplitOptions.None);
                    line = split[0];
                    //remove space between comment and code
                    //line = line.TrimEnd(' ');
                }

                line = line.Trim();

                // In the event of a +Trigger, if we are force-lowercasing it, then do so
                // now before the syntax check.
                if (forceCase && cmd == CMD_TRIGGER)
                    line = line.ToLower();


                //Run a syntax check on this line
                try
                {
                    checkSyntax(cmd, line);
                }
                catch (ParserException pex)
                {
                    if (strict)
                        throw pex;
                    else
                        logger.warn($"Syntax logger.error(: {pex.Message} at {filename} line {lineno} near {cmd} {line}");
                }


                // Reset the %Previous if this is a new +Trigger.
                if (cmd.Equals(CMD_TRIGGER))
                {
                    previous = "";
                }

                // Do a look-ahead to see ^Continue and %Previous.
                if (cmd != CMD_CONTINUE)
                {
                    for (int j = (i + 1); j < code.Length; j++)
                    {
                        // Peek ahead.
                        string peek = Util.Strip(code[j]);

                        // Skip blank. 
                        if (peek.Length == 0) //peek.Length < 2?
                        {
                            continue;
                        }

                        // Get the command.
                        string peekCmd = peek.Substring(0, 1);
                        peek = Util.Strip(peek.Substring(1));

                        // Only continue if the lookahead line has any data.
                        if (peek.Length > 0)
                        {
                            // The lookahead command has to be a % or a ^
                            if (!peekCmd.Equals(CMD_CONTINUE) && !peekCmd.Equals(CMD_PREVIOUS))
                            {
                                break;
                            }

                            // If the current command is a +, see if the following is a %.
                            if (cmd.Equals(CMD_TRIGGER))
                            {
                                if (peekCmd.Equals(CMD_PREVIOUS))
                                {
                                    // It has a %Previous!
                                    previous = peek;
                                    break;
                                }
                                else
                                {
                                    previous = "";
                                }
                            }

                            // If the current command is a ! and the next command(s) are
                            // ^, we'll tack each extension on as a "line break".
                            if (cmd.Equals(CMD_DEFINE))
                            {
                                if (peekCmd.Equals(CMD_CONTINUE))
                                {
                                    line += "<crlf>" + peek;
                                }
                            }

                            // If the current command is not a ^ and the line after is
                            // not a %, but the line after IS a ^, then tack it onto the
                            // end of the current line.
                            if (!cmd.Equals(CMD_CONTINUE) && !cmd.Equals(CMD_PREVIOUS) && !cmd.Equals(CMD_DEFINE))
                            {
                                if (peekCmd.Equals(CMD_CONTINUE))
                                {
                                    // Concatenation character?
                                    string concatMode = local_options["concat"];
                                    string concatChar = "";
                                    if (concatMode != null && concatModes.ContainsKey(concatMode))
                                        concatChar = concatModes[concatMode];

                                    line += concatChar + peek;
                                }
                                else
                                {
                                    break; //?warn
                                }
                            }
                        }
                    }
                }

                // Start handling command types.
                //TODO: change to switch-case
                if (cmd.Equals(CMD_DEFINE))
                {
                    logger.debug("\t! DEFINE");
                    //string[] halves = line.split("\\s*=\\s*", 2);
                    string[] halves = new Regex("\\s*=\\s*").Split(line, 2);
                    //string[] left = whatis[0].split("\\s+", 2);
                    string[] left = new Regex("\\s+").Split(halves[0], 2);
                    string value = "";
                    string kind = ""; //global, var, sub, ...
                    string name = "";
                    bool delete = false;

                    if (halves.Length == 2)
                    {
                        value = halves[1].Trim();
                    }

                    if (left.Length >= 1)
                    {
                        kind = left[0];
                        if (left.Length >= 2)
                        {
                            left = Util.CopyOfRange(left, 1, left.Length);
                            //name = left[1].Trim().ToLower();
                            name = Util.Join(left, " ").Trim();
                        }
                    }

                    // Remove line breaks unless this is an array.
                    if (!kind.Equals("array"))
                    {
                        value = value.Replace("<crlf>", "");
                    }

                    // Version is the only type that doesn't have a var.
                    if (kind.Equals("version"))
                    {
                        logger.debug("\tUsing RiveScript version " + value);

                        // Convert the value into a double, catch exceptions.
                        double version = 0;
                        try
                        {
                            version = double.Parse(value ?? "", new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                        }
                        catch (FormatException)
                        {
                            logger.warn("RiveScript version \"" + value + "\" not a valid floating number", filename, lineno);
                            continue;
                        }

                        if (version > RS_VERSION)
                        {
                            throw new ParserException($"We can't parse RiveScript v {value} documents at {filename} line {lineno}. Only support {RS_VERSION}.");
                        }

                        continue;
                    }
                    else
                    {
                        // All the other types require a variable and value.
                        if (name.Equals(""))
                        {
                            logger.warn("Missing a " + kind + " variable name", filename, lineno);
                            continue;
                        }
                        if (value.Equals(""))
                        {
                            logger.warn("Missing a " + kind + " value", filename, lineno);
                            continue;
                        }
                        if (value.Equals(Constants.UNDEF_TAG))
                        {
                            // Deleting its value.
                            delete = true;
                        }
                    }

                    // Handle the variable set types.
                    //TODO: change to switch-case
                    if (kind.Equals("local"))
                    {
                        // Local file scoped parser options
                        logger.debug("\tSet local parser option " + name + " = " + value);
                        local_options.AddOrUpdate(name, value);
                    }
                    else if (kind.Equals("global"))
                    {
                        // Is it a special global? (debug or depth or etc).
                        logger.debug("\tSet global " + name + " = " + value);
                        ast.begin.addGlobals(name, value);
                    }
                    else if (kind.Equals("var"))
                    {
                        // Set a bot variable.
                        logger.debug("\tSet bot variable " + name + " = " + value);
                        ast.begin.addVar(name, value);
                    }
                    else if (kind.Equals("array"))
                    {
                        // Set an array.
                        logger.debug("\tSet array " + name);

                        // Deleting it?
                        if (delete)
                        {
                            ast.begin.removeArray(name);
                            continue;
                        }

                        // Did the array have multiple lines?
                        //string[] parts = value.split("<crlf>");
                        //WARN:
                        string[] parts = value.Split("<crlf>");
                        ICollection<string> items = new List<string>();
                        for (int a = 0; a < parts.Length; a++)
                        {
                            // Split at pipes or spaces?
                            string[] pieces;
                            if (parts[a].IndexOf("|") > -1)
                            {
                                //pieces = parts[a].split("\\|");
                                pieces = new Regex("\\|").Split(parts[a]);
                            }
                            else
                            {
                                pieces = new Regex("\\s+").Split(parts[a]);
                            }

                            // Add the pieces to the final array.
                            for (int b = 0; b < pieces.Length; b++)
                            {
                                items.Add(pieces[b]);
                            }
                        }

                        // Store this array.
                        ast.begin.addArray(name, items);
                    }
                    else if (kind.Equals("sub"))
                    {
                        // Set a substitution.
                        logger.debug("\tSubstitution " + name + " => " + value);
                        ast.begin.addSub(name, value);
                    }
                    else if (kind.Equals("person"))
                    {
                        // Set a person substitution.
                        logger.debug("\tPerson substitution " + name + " => " + value);
                        ast.begin.addPerson(name, value);
                    }
                    else
                    {
                        logger.warn("Unknown definition type \"" + kind + "\"", filename, lineno);
                        continue;
                    }
                }
                else if (cmd.Equals(CMD_LABEL))
                {
                    // > LABEL
                    logger.debug("\t> LABEL");
                    //string[] label =  line.split("\\s+");
                    string[] label = line.SplitRegex("\\s+");
                    string type = "";
                    string name = "";
                    if (label.Length >= 1)
                    {
                        type = label[0].Trim().ToLower();
                    }
                    if (label.Length >= 2)
                    {
                        name = label[1].Trim();
                    }

                    // Handle the label types.
                    if (type.Equals("begin"))
                    {
                        // The BEGIN statement.
                        logger.debug("\tFound the BEGIN Statement.");

                        // A BEGIN is just a special topic.
                        type = "topic";
                        name = "__begin__";
                    }

                    if (type.Equals("topic"))
                    {
                        if (forceCase)
                            name = name.ToLower();


                        // Starting a new topic.
                        logger.debug("\tSet topic to " + name);
                        currentTrigger = "";
                        currentTriggerObject = null;
                        topic = name;

                        // Does this topic include or inherit another one?
                        if (label.Length >= 3)
                        {
                            int mode_includes = 1;
                            int mode_inherits = 2;
                            int mode = 0;
                            for (int a = 2; a < label.Length; a++)
                            {
                                if (label[a].ToLowerInvariant().Equals("includes"))
                                {
                                    mode = mode_includes;
                                }
                                else if (label[a].ToLowerInvariant().Equals("inherits"))
                                {
                                    mode = mode_inherits;
                                }
                                else if (mode > 0)
                                {
                                    // This topic is either inherited or included.
                                    if (mode == mode_includes)
                                    {
                                        ast.getTopic(topic).includes(label[a]);
                                    }
                                    else if (mode == mode_inherits)
                                    {
                                        ast.getTopic(topic).inherits(label[a]);
                                    }
                                }
                            }
                        }
                    }
                    if (type.Equals("object"))
                    {
                        // If a field was provided, it should be the programming language.
                        string language = "";

                        if (label.Length >= 3)
                        {
                            language = label[2].ToLower();
                        }

                        // Only try to parse a language we support.
                        currentTrigger = "";
                        if (language.Length == 0)
                        {
                            logger.warn("Trying to parse unknown programming language (assuming it's CSharp)", filename, lineno);
                            language = Constants.CSharpHandlerName; // Assume it's JavaScript
                        }

                        //INFO: to remove?
                        //if (!handlers.ContainsKey(language))
                        //{
                        //    // We don't have a handler for this language.
                        //    logger.debug("We can't handle " + language + " object code!");
                        //    continue;
                        //}

                        // Start collecting its code!
                        objectName = name;
                        objectLanguage = language;
                        objectBuffer = new List<string>();
                        inObject = true;
                    }
                }
                else if (cmd.Equals(CMD_ENDLABEL))
                {
                    // < ENDLABEL
                    logger.debug("\t< ENDLABEL");
                    string type = line.Trim().ToLower();

                    if (type.Equals("begin") || type.Equals("topic"))
                    {
                        logger.debug("\t\tEnd topic label.");
                        topic = "random";
                    }
                    else if (type.Equals("object"))
                    {
                        logger.debug("\t\tEnd object label.");
                        inObject = false;
                    }
                    else
                    {
                        logger.warn("Unknown end topic type \"" + type + "\"", filename, lineno);
                    }
                }
                else if (cmd.Equals(CMD_TRIGGER))
                {
                    // + TRIGGER
                    logger.debug("\t+ TRIGGER pattern: " + line);
                    currentTriggerObject = new Trigger(line);

                    if (previous.Length > 0)
                    {
                        currentTrigger = line + "{previous}" + previous;
                        currentTriggerObject.setPrevious(true);
                    }
                    else
                    {
                        currentTrigger = line;
                    }

                    ast.getTopic(topic).addTrigger(currentTriggerObject);

                    //TODO onld stuff to see
                    //if (previous.Length > 0)
                    //{
                    //    // This trigger had a %Previous. To prevent conflict, tag the
                    //    // trigger with the "that" text.
                    //    currentTrigger = line + "{previous}" + previous;

                    //    topics.topic(topic).trigger(line).hasPrevious(true);
                    //    topics.topic(topic).addPrevious(line, previous);
                    //}
                    //else
                    //{
                    //    // Set the current trigger to this.
                    //    currentTrigger = line;
                    //}
                }
                else if (cmd.Equals(CMD_REPLY))
                {
                    // - REPLY
                    logger.debug("\t- REPLY: " + line);

                    // This can't come before a trigger!
                    if (currentTriggerObject == null)
                    {
                        logger.warn("Reply found before trigger", filename, lineno);
                        continue;
                    }

                    // Warn if we also saw a hard redirect.
                    if (currentTriggerObject.hasRedirect())
                    {
                        logger.warn("You can't mix @Redirects with -Replies", filename, lineno);
                    }

                    // Add the reply to the trigg
                    currentTriggerObject.addReply(line);
                }
                else if (cmd.Equals(CMD_PREVIOUS))
                {
                    // % PREVIOUS
                    // This was handled above.
                    continue;
                }
                else if (cmd.Equals(CMD_CONTINUE))
                {
                    // ^ CONTINUE
                    // This was handled above.
                    continue;
                }
                else if (cmd.Equals(CMD_REDIRECT))
                {
                    // @ REDIRECT
                    logger.debug("\t@ REDIRECT: " + line);

                    // This can't come before a trigger!
                    if (currentTrigger.Length == 0)
                    {
                        logger.warn("Redirect found before trigger", filename, lineno);
                        continue;
                    }

                    // Add the redirect to the trigger.
                    // TODO: this extends RiveScript, not compat w/ Perl yet
                    currentTriggerObject.addRedirect(line);
                }
                else if (cmd.Equals(CMD_CONDITION))
                {
                    // * CONDITION
                    logger.debug("\t* CONDITION: " + line);

                    // This can't come before a trigger!
                    if (currentTriggerObject == null)
                    {
                        logger.warn("Redirect found before trigger", filename, lineno);
                        continue;
                    }

                    // Add the condition to the trigger.
                    currentTriggerObject.addCondition(line);
                }
                else
                {
                    logger.warn("Unrecognized command \"" + cmd + "\"", filename, lineno);
                }
            }


            if (logger.isDebugEnable)
            {
                logger.debug($"Parsing {filename} completed in {DateTime.Now.Ticks - startTime} ms");
            }

            return ast;
        }



        private void checkSyntax(string cmd, string line)
        {
            //Run syntax tests based on the command used.

            if (cmd == "!")
            {
                // ! Definition
                // - Must be formatted like this:
                //   ! type name = value
                //   OR
                //   ! type = value
                if (!line.MatchRegex(@"^.+(?:\s +.+|)\s*=\s*.+?$"))
                {
                    throw new ParserException("Invalid format for !Definition line: must be '! type name = value' OR '! type = value'");
                }
            }
            else if (cmd == ">")
            {
                // > Label
                // - The "begin" label must have only one argument ("begin")
                // - The "topic" label must be lowercased but can inherit other topics
                // - The "object" label must follow the same rules as "topic", but don't
                //   need to be lowercased.
                var parts = line.SplitRegex(@"\s+");

                if (parts[0] == "begin" && parts.Length > 1)
                {
                    throw new ParserException("The 'begin' label takes no additional arguments");
                }
                else if (parts[0] == "topic")
                {
                    if (forceCase && line.MatchRegex(@"[^a-z0-9_\-\s]"))
                    {
                        throw new ParserException("Topics should be lowercased and contain only letters and numbers");
                    }
                    else if (line.MatchRegex(@"[^A-Za-z0-9_\-\s]"))
                    {
                        throw new ParserException("Topics should contain only letters and numbers in forceCase mode");
                    }
                }
                else if (parts[0] == "object")
                {
                    if (line.MatchRegex(@"[^A-Za-z0-9_\-\s]"))
                    {
                        throw new ParserException("Objects can only contain numbers and letters");
                    }
                }
            }
            else if (cmd == "+" || cmd == "%" || cmd == "@")
            {
                // + Trigger, % Previous, @ Redirect
                // This one is strict. The triggers are to be run through the regexp
                // engine, therefore it should be acceptable for the regexp engine.
                // - Entirely lowercase
                // - No symbols except: ( | ) [ ] * _ // { } < > =
                // - All brackets should be matched.
                var parens = 0;
                var square = 0;
                var curly = 0;
                var angle = 0; // Count the brackets

                // Look for obvious errors first.
                if (utf8)
                {
                    // In UTF-8 mode, most symbols are allowed.
                    if (line.MatchRegex("[A-Z\\.]"))
                    {
                        throw new ParserException("Triggers can't contain uppercase letters, backslashes or dots in UTF - 8 mode");
                    }
                }
                else if (line.MatchRegex(@"[^a-z0-9(|)\[\]*_//@{}<>=\s]"))
                {
                    throw new ParserException("Triggers may only contain lowercase letters, numbers, and these symbols: ( | )[ ] * _ // { } < > =");
                }

                // Count the brackets.
                foreach (char c in line.ToCharArray())
                {

                    switch (c)
                    {
                        case '(': parens++; break;
                        case ')': parens--; break;
                        case '[': square++; break;
                        case ']': square--; break;
                        case '{': curly++; break;
                        case '}': curly--; break;
                        case '<': angle++; break;
                        case '>': angle--; break;
                        default: break;
                    }

                }


                // Any mismatches?
                if (parens != 0)
                    throw new ParserException("Unmatched parenthesis brackets");
                if (square != 0)
                    throw new ParserException("Unmatched square brackets");
                if (curly != 0)
                    throw new ParserException("Unmatched curly brackets");
                if (angle != 0)
                    throw new ParserException("Unmatched angle brackets");
            }
            else if (cmd == "*")
            {
                // * Condition
                // Syntax for a conditional is as follows:
                // * value symbol value => response
                if (!line.MatchRegex(@"^.+?\s*(?:==|eq|!=|ne|<>|<|<=|>|>=)\s*.+?=>.+?$"))
                {
                    throw new ParserException("Invalid format for !Condition: should be like '* value symbol value => response'");
                }
            }

            // No problems!
        }
    }
}
