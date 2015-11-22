using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RiveScript
{
    public class RiveScript
    {
        // Private class variables.
        private bool debug = false;                 // Debug mode
        private int depth = 50;                     // Recursion depth limit
        private string _error = "";                  // Last error text
        private static Random rand = new Random();  // A random number generator


        //The version of the RiveScript C# library.
        public static readonly double VERSION = 0.01;

        // Constant RiveScript command symbols.
        private static readonly double RS_VERSION = 2.0; // This implements RiveScript 2.0
        private static readonly string CMD_DEFINE = "!";
        private static readonly string CMD_TRIGGER = "+";
        private static readonly string CMD_PREVIOUS = "%";
        private static readonly string CMD_REPLY = "-";
        private static readonly string CMD_CONTINUE = "^";
        private static readonly string CMD_REDIRECT = "@";
        private static readonly string CMD_CONDITION = "*";
        private static readonly string CMD_LABEL = ">";
        private static readonly string CMD_ENDLABEL = "<";



        // The topic data structure, and the "thats" data structure.
        private TopicManager topics = new TopicManager();

        // Bot's users' data structure.
        private ClientManager clients = new ClientManager();

        // Object handlers //TODO: At this moment, this the lib is not able to do this
        private IDictionary<string, IObjectHandler> handlers = new Dictionary<string, IObjectHandler>();
        private IDictionary<string, string> objects = new Dictionary<string, string>(); // name->language mappers

        // Simpler internal data structures.
        private ICollection<string> listTopics = new List<string>();                                                // vector containing topic list (for quicker lookups)
        private IDictionary<string, string> globals = new Dictionary<string, string>();                             // ! global
        private IDictionary<string, string> vars = new Dictionary<string, string>();                                // ! var
        private IDictionary<string, ICollection<string>> arrays = new Dictionary<string, ICollection<string>>();    // ! array
        private IDictionary<string, string> subs = new Dictionary<string, string>();                                // ! sub
        private string[] subs_s = null;                                                                             // sorted subs
        private IDictionary<string, string> person = new Dictionary<string, string>();                              // ! person
        private string[] person_s = null;                                                                           // sorted persons


        /// <summary>
        /// Create a new RiveScript interpreter object, specifying the debug mode.
        /// </summary>
        /// <param name="debug">Is true Enable debug mode (a *lot* of stuff is printed to the terminal)</param>
        public RiveScript(bool debug)
        {
            this.debug = debug;
            // Set static debug modes.
            Topic.setDebug(debug);
        }

        /// <summary>
        /// Create a new RiveScript interpreter object with debug disabled.
        /// </summary>
        public RiveScript() : this(false) { }

        /// <summary>
        /// Return the text of the last error message given.
        /// </summary>
        public string error()
        {
            return this._error;
        }

        /// <summary>
        /// Set the error message.
        /// </summary>
        /// <param name="message">The new error message to set.</param>
        /// <returns></returns>
        protected bool error(string message)
        {
            _error = message;
            return false;
        }

        #region Load Methods

        /// <summary>
        /// Load a directory full of RiveScript documents, specifying a custom
	    /// list of valid file extensions.
        /// </summary>
        /// <param name="path">The path to the directory containing RiveScript documents.</param>
        /// <param name="exts">A string array containing file extensions to look for.</param>
        /// <returns></returns>
        public bool loadDirectory(string path, string[] exts)
        {
            say("Load directory: " + path);

            // Verify Directory
            if (false == Directory.Exists(path))
                return error("Directory not found");

            //Adjust exts for all have .
            for (int i = 0; i < exts.Length; i++)
            {
                if (false == exts[i].StartsWith("."))
                {
                    exts[i] = "." + exts[i];
                }
            }

            //TODO: I can Do better - I try avoid Linq

            // Search it for files recursive
            var files = new List<string>();
            foreach (var f in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var fi = new FileInfo(f);
                if (Array.IndexOf(exts, fi.Extension) > -1)
                {
                    files.Add(fi.FullName);
                }
            }

            // No results?
            if (files.Count == 0)
            {
                return error("Couldn't read any files from directory " + path);
            }

            // Parse each file.
            foreach (var file in files)
            {
                loadFile(path + "/" + file);
            }

            return true;
        }

        /// <summary>
        ///  Load a directory full of RiveScript documents (.rive or .rs files).
        /// </summary>
        /// <param name="path">The path to the directory containing RiveScript documents.</param>
        /// <returns></returns>
        public bool loadDirectory(string path)
        {
            return loadDirectory(path, new[] { ".rive", ".rs" });
        }

        /// <summary>
        /// Load a single RiveScript document.
        /// </summary>
        /// <param name="file">file Path to a RiveScript document.</param>
        private bool loadFile(string file)
        {
            say("Load file: " + file);

            // Run some sanity checks on the file handle.
            if (false == File.Exists(file)) //Verify is is a file and is exists
                return error(file + ": file not found.");

            if (false == FileHelper.CanRead(file))
                return error(file + ": can't read from file.");


            // Slurp the file's contents.
            string[] lines;

            try
            {
                //QUESTION: With ou without UTF8??
                //lines = File.ReadAllLines(file);
                lines = File.ReadAllLines(file, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                // How did this happen? We checked it earlier.
                return error(file + ": file not found exception.");
            }
            catch (IOException e)
            {
                trace(e);
                return error(file + ": IOException while reading.");
            }


            // Send the code to the parser.
            return parse(file, lines);
        }

        /// <summary>
        /// Stream some RiveScript code directly into the interpreter (as a single string
        /// containing newlines in it).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool stream(string code)
        {
            // Split the given code up into lines.
            var lines = code.Split(new[] { "\n" }, StringSplitOptions.None);

            // Send the lines to the parser.
            return parse("(streamed)", lines);
        }

        /// <summary>
        /// Stream some RiveScript code directly into the interpreter (as a string array,
        /// one line per item).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool stream(string[] code)
        {
            // The coder has already broken the lines for us!
            return parse("(streamed)", code);
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Add a handler for a programming language to be used with RiveScript object calls.
        /// </summary>
        /// <param name="name">The name of the programming language.</param>
        /// <param name="handler">An instance of a class that implements an ObjectHandler.</param>
        public void setHandler(string name, IObjectHandler handler)
        {
            handlers.Add(name, handler);
        }

        /// <summary>
        /// Set a global variable for the interpreter (equivalent to ! global).
        /// Set the value to null to delete the variable.<p>
        ///
        /// There are two special globals that require certain data types:<p>
        ///
        /// debug is bool-like and its value must be a string value containing
        /// "true", "yes", "1", "false", "no" or "0".<p>
        ///
        /// depth is integer-like and its value must be a quoted integer like "50".
        /// The "depth" variable controls how many levels deep RiveScript will go when
        /// following reply redirections.<p>
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable's value.</param>
        /// <returns></returns>
        public bool setGlobal(string name, string value)
        {
            var delete = false;
            if (value == null || value == "<undef>")
            {
                delete = true;
            }

            // Special globals
            if (name == "debug")
            {
                // Debug is a bool.
                if (Util.IsTrue(value))
                {
                    this.debug = true;
                }
                else if (Util.IsFalse(value) || delete)
                {
                    this.debug = false;
                }
                else
                {
                    return error("Global variable \"debug\" needs a bool value");
                }
            }
            else if (name == "depth")
            {
                // Depth is an integer.
                try
                {
                    this.depth = int.Parse(value);
                }
                catch (FormatException)
                {
                    return error("Global variable \"depth\" needs an integer value");
                }
                catch (OverflowException)
                {
                    return error("Global variable \"depth\" is to long");
                }
            }

            // It's a user-defined global. OK.
            if (delete)
            {
                globals.Remove(name);
            }
            else
            {
                globals.Add(name, value);
            }

            return true;
        }

        /// <summary>
        /// Set a bot variable for the interpreter (equivalent to ! var). A bot
        /// variable is data about the chatbot, like its name or favorite color.<p>
        ///
        /// A null value will delete the variable.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable's value.</param>
        /// <returns></returns>
        public bool setVariable(string name, string value)
        {
            if (value == null || value == "<undef>")
            {
                vars.Remove(name);
            }
            else
            {
                vars.Add(name, value);
            }

            return true;
        }

        /// <summary>
        /// Set a substitution pattern (equivalent to ! sub). The user's input (and
        /// the bot's reply, in %Previous) get substituted using these rules.<p>
        ///
        /// A null value for the output will delete the substitution.
        /// </summary>
        /// <param name="pattern">The pattern to match in the message.</param>
        /// <param name="output">The text to replace it with (must be lowercase, no special characters)</param>
        /// <returns></returns>
        public bool setSubstitution(string pattern, string output)
        {
            if (output == null || output == "<undef>")
            {
                subs.Remove(pattern);
            }
            else
            {
                subs.Add(pattern, output);
            }

            return true;
        }

        /// <summary>
        ///  Set a person substitution pattern (equivalent to ! person).
        ///  substitutions swap first- and second-person pronouns, so th
        ///  safely echo the user without sounding too mechanical.<p>
        /// 
        ///  A null value for the output will delete the substitution.
        /// </summary>
        /// <param name="pattern">The pattern to match in the message.</param>
        /// <param name="output">The text to replace it with (must be lowercase, no special characters).</param>
        /// <returns></returns>
        public bool setPersonSubstitution(string pattern, string output)
        {
            if (output == null || output == "<undef>")
            {
                person.Remove(pattern);
            }
            else
            {
                person.Add(pattern, output);
            }

            return true;
        }

        /// <summary>
        ///  Set a variable for one of the bot's users. A null value will delete a
        ///  variable.
        /// </summary>
        /// <param name="user">The user's ID.</param>
        /// <param name="name">The name of the variable to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns></returns>
        public bool setUservar(string user, string name, string value)
        {
            if (value == null || value == "<undef>")
            {
                clients.Client(user).delete(name);
            }
            else
            {
                clients.Client(user).set(name, value);
            }

            return true;
        }

        /// <summary>
        /// Set -all- user vars for a user. This will replace the internal hash for
        /// the user. So your hash should at least contain a key/value pair for the
        /// user's current "topic". This could be useful if you used getUservars to
        /// store their entire profile somewhere and want to restore it later.
        /// </summary>
        /// <param name="user">The user's ID.</param>
        /// <param name="data">The full hash of the user's data.</param>
        /// <returns></returns>
        public bool setUservars(string user, IDictionary<string, string> data)
        {
            // TODO: this should be handled more sanely. ;)
            clients.Client(user).replaceData(data);
            return true;
        }

        /// <summary>
        /// Get a list of all the user IDs the bot knows about.
        /// </summary>
        public string[] getUsers()
        {
            // Get the user list from the clients object.
            return clients.listClients();
        }

        /// <summary>
        ///  Retrieve a listing of all the uservars for a user as a HashMap.
        /// Returns null if the user doesn't exist.
        /// </summary>
        /// <param name="user">The user ID to get the vars for.</param>
        /// <returns></returns>
        public IDictionary<string, string> getUserVars(string user)
        {
            if (clients.clientExists(user))
            {
                return clients.Client(user).getData;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve a single variable from a user's profile.
        ///
        /// Returns null if the user doesn't exist. Returns the string "undefined"
        /// if the variable doesn't exist.
        /// </summary>
        /// <param name="user">The user ID to get data from.</param>
        /// <param name="name">The name of the variable to get.</param>
        /// <returns></returns>
        public string getUserVar(string user, string name)
        {
            if (clients.clientExists(user))
            {
                return clients.Client(user).get(name);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region  Parsing Methods

        private bool parse(string filename, string[] code)
        {
            // Track some state variables for this parsing round.
            string topic = "random"; // Default topic = random
            int lineno = 0;
            bool comment = false; // In a multi-line comment
            bool inobj = false; // In an object
            string objName = "";    // Name of the current object
            string objLang = "";    // Programming language of the object
            ICollection<string> objBuff = null;  // Buffer for the current object
            string onTrig = "";    // Trigger we're on
            //string lastcmd = "";    // Last command code //Never Used
            string isThat = "";    // Is a %Previous trigger

            // File scoped parser options.
            IDictionary<string, string> local_options = new Dictionary<string, string>();
            local_options.Add("concat", "none");

            // The given "code" is an array of lines, so jump right in.
            for (int i = 0; i < code.Length; i++)
            {
                lineno++; // Increment the line counter.
                string line = code[i];
                say("Line: " + line);

                // Trim the line of whitespaces.
                line = line.Trim();

                // Are we inside an object?
                if (inobj)
                {
                    if (line.StartsWith("<object") || line.StartsWith("< object"))
                    { // TODO regexp
                      // End of the object. Did we have a handler?
                        if (handlers.ContainsKey(objLang))
                        {
                            // Yes, call the handler's onLoad function.
                            handlers[objLang].onLoad(objName, objBuff.ToArray());

                            // Map the name to the language.
                            objects.Add(objName, objLang);
                        }

                        objName = "";
                        objLang = "";
                        objBuff = null;
                        inobj = false;
                        continue;
                    }

                    // Collect the code.
                    objBuff.Add(line);
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
                    comment = true;
                }
                else if (line.StartsWith("/"))
                {
                    // A single line comment.
                    continue;
                }
                else if (line.IndexOf("*/") > -1)
                {
                    // End a multi-line comment.
                    comment = false;
                    continue;
                }
                if (comment)
                {
                    continue;
                }

                // Skip any blank lines.
                if (line.Length < 2)
                {
                    continue;
                }

                // Separate the command from the rest of the line.
                string cmd = line.Substring(0, 1);
                line = line.Substring(1).Trim();
                say("\tCmd: " + cmd);

                // Ignore inline comments.
                if (line.IndexOf(" // ") > -1)
                {

                    string[] split = line.Split(new[] { " // " }, StringSplitOptions.None);
                    line = split[0];
                }

                // Reset the %Previous if this is a new +Trigger.
                if (cmd.Equals(CMD_TRIGGER))
                {
                    isThat = "";
                }

                // Do a look-ahead to see ^Continue and %Previous.
                for (int j = (i + 1); j < code.Length; j++)
                {
                    // Peek ahead.
                    string peek = code[j].Trim();

                    // Skip blank.
                    if (peek.Length == 0)
                    {
                        continue;
                    }

                    // Get the command.
                    string peekCmd = peek.Substring(0, 1);
                    peek = peek.Substring(1).Trim();

                    // Only continue if the lookahead line has any data.
                    if (peek.Length > 0)
                    {
                        // The lookahead command has to be a % or a ^
                        if (peekCmd.Equals(CMD_CONTINUE) == false && peekCmd.Equals(CMD_PREVIOUS) == false)
                        {
                            break;
                        }

                        // If the current command is a +, see if the following is a %.
                        if (cmd.Equals(CMD_TRIGGER))
                        {
                            if (peekCmd.Equals(CMD_PREVIOUS))
                            {
                                // It has a %Previous!
                                isThat = peek;
                                break;
                            }
                            else
                            {
                                isThat = "";
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
                        if (cmd.Equals(CMD_CONTINUE) == false && cmd.Equals(CMD_PREVIOUS) == false && cmd.Equals(CMD_DEFINE) == false)
                        {
                            if (peekCmd.Equals(CMD_CONTINUE))
                            {
                                // Concatenation character?
                                string concat = "";
                                if (local_options["concat"].Equals("space"))
                                {
                                    concat = " ";
                                }
                                else if (local_options["concat"].Equals("newline"))
                                {
                                    concat = "\n";
                                }
                                line += concat + peek;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                // Start handling command types.
                if (cmd.Equals(CMD_DEFINE))
                {
                    say("\t! DEFINE");
                    //string[] whatis = line.split("\\s*=\\s*", 2);
                    string[] whatis = new Regex("\\s*=\\s*").Split(line, 2);
                    //string[] left = whatis[0].split("\\s+", 2);
                    string[] left = new Regex("\\s+").Split(whatis[0], 2);
                    string type = left[0];
                    string var = "";
                    string value = "";
                    bool delete = false;
                    if (left.Length == 2)
                    {
                        var = left[1].Trim().ToLower();
                    }
                    if (whatis.Length == 2)
                    {
                        value = whatis[1].Trim();
                    }

                    // Remove line breaks unless this is an array.
                    if (!type.Equals("array"))
                    {
                        value = value.Replace("<crlf>", "");
                    }

                    // Version is the only type that doesn't have a var.
                    if (type.Equals("version"))
                    {
                        say("\tUsing RiveScript version " + value);

                        // Convert the value into a double, catch exceptions.
                        double version = 0;
                        try
                        {
                            version = double.Parse(value);
                        }
                        catch (FormatException)
                        {
                            cry("RiveScript version \"" + value + "\" not a valid floating number", filename, lineno);
                            continue;
                        }

                        if (version > RS_VERSION)
                        {
                            cry("We can't parse RiveScript v" + value + " documents", filename, lineno);
                            return false;
                        }

                        continue;
                    }
                    else
                    {
                        // All the other types require a variable and value.
                        if (var.Equals(""))
                        {
                            cry("Missing a " + type + " variable name", filename, lineno);
                            continue;
                        }
                        if (value.Equals(""))
                        {
                            cry("Missing a " + type + " value", filename, lineno);
                            continue;
                        }
                        if (value.Equals("<undef>"))
                        {
                            // Deleting its value.
                            delete = true;
                        }
                    }

                    // Handle the variable set types.
                    if (type.Equals("local"))
                    {
                        // Local file scoped parser options
                        say("\tSet local parser option " + var + " = " + value);
                        local_options.Add(var, value);
                    }
                    else if (type.Equals("global"))
                    {
                        // Is it a special global? (debug or depth or etc).
                        say("\tSet global " + var + " = " + value);
                        this.setGlobal(var, value);
                    }
                    else if (type.Equals("var"))
                    {
                        // Set a bot variable.
                        say("\tSet bot variable " + var + " = " + value);
                        this.setVariable(var, value);
                    }
                    else if (type.Equals("array"))
                    {
                        // Set an array.
                        say("\tSet array " + var);

                        // Deleting it?
                        if (delete)
                        {
                            arrays.Remove(var);
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
                        arrays.Add(var, items);
                    }
                    else if (type.Equals("sub"))
                    {
                        // Set a substitution.
                        say("\tSubstitution " + var + " => " + value);
                        this.setSubstitution(var, value);
                    }
                    else if (type.Equals("person"))
                    {
                        // Set a person substitution.
                        say("\tPerson substitution " + var + " => " + value);
                        this.setPersonSubstitution(var, value);
                    }
                    else
                    {
                        cry("Unknown definition type \"" + type + "\"", filename, lineno);
                        continue;
                    }
                }
                else if (cmd.Equals(CMD_LABEL))
                {
                    // > LABEL
                    say("\t> LABEL");
                    //string[] label =  line.split("\\s+");
                    string[] label = new Regex("\\s+").Split(line);
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
                        say("\tFound the BEGIN Statement.");

                        // A BEGIN is just a special topic.
                        type = "topic";
                        name = "__begin__";
                    }
                    if (type.Equals("topic"))
                    {
                        // Starting a new topic.
                        say("\tSet topic to " + name);
                        onTrig = "";
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
                                        topics.topic(topic).includes(label[a]);
                                    }
                                    else if (mode == mode_inherits)
                                    {
                                        topics.topic(topic).inherits(label[a]);
                                    }
                                }
                            }
                        }
                    }
                    if (type.Equals("object"))
                    {
                        // If a field was provided, it should be the programming language.
                        string lang = "";
                        if (label.Length >= 3)
                        {
                            lang = label[2].ToLower();
                        }

                        // Only try to parse a language we support.
                        onTrig = "";
                        if (lang.Length == 0)
                        {
                            cry("Trying to parse unknown programming language (assuming it's JavaScript)", filename, lineno);
                            lang = "javascript"; // Assume it's JavaScript
                        }
                        if (!handlers.ContainsKey(lang))
                        {
                            // We don't have a handler for this language.
                            say("We can't handle " + lang + " object code!");
                            continue;
                        }

                        // Start collecting its code!
                        objName = name;
                        objLang = lang;
                        objBuff = new List<string>();
                        inobj = true;
                    }
                }
                else if (cmd.Equals(CMD_ENDLABEL))
                {
                    // < ENDLABEL
                    say("\t< ENDLABEL");
                    string type = line.Trim().ToLower();

                    if (type.Equals("begin") || type.Equals("topic"))
                    {
                        say("\t\tEnd topic label.");
                        topic = "random";
                    }
                    else if (type.Equals("object"))
                    {
                        say("\t\tEnd object label.");
                        inobj = false;
                    }
                    else
                    {
                        cry("Unknown end topic type \"" + type + "\"", filename, lineno);
                    }
                }
                else if (cmd.Equals(CMD_TRIGGER))
                {
                    // + TRIGGER
                    say("\t+ TRIGGER: " + line);

                    if (isThat.Length > 0)
                    {
                        // This trigger had a %Previous. To prevent conflict, tag the
                        // trigger with the "that" text.
                        onTrig = line + "{previous}" + isThat;
                        topics.topic(topic).trigger(line).hasPrevious(true);
                        topics.topic(topic).addPrevious(line, isThat);
                    }
                    else
                    {
                        // Set the current trigger to this.
                        onTrig = line;
                    }
                }
                else if (cmd.Equals(CMD_REPLY))
                {
                    // - REPLY
                    say("\t- REPLY: " + line);

                    // This can't come before a trigger!
                    if (onTrig.Length == 0)
                    {
                        cry("Reply found before trigger", filename, lineno);
                        continue;
                    }

                    // Add the reply to the trigger.
                    topics.topic(topic).trigger(onTrig).addReply(line);
                }
                else if (cmd.Equals(CMD_PREVIOUS))
                {
                    // % PREVIOUS
                    // This was handled above.
                }
                else if (cmd.Equals(CMD_CONTINUE))
                {
                    // ^ CONTINUE
                    // This was handled above.
                }
                else if (cmd.Equals(CMD_REDIRECT))
                {
                    // @ REDIRECT
                    say("\t@ REDIRECT: " + line);

                    // This can't come before a trigger!
                    if (onTrig.Length == 0)
                    {
                        cry("Redirect found before trigger", filename, lineno);
                        continue;
                    }

                    // Add the redirect to the trigger.
                    // TODO: this extends RiveScript, not compat w/ Perl yet
                    topics.topic(topic).trigger(onTrig).addRedirect(line);
                }
                else if (cmd.Equals(CMD_CONDITION))
                {
                    // * CONDITION
                    say("\t* CONDITION: " + line);

                    // This can't come before a trigger!
                    if (onTrig.Length == 0)
                    {
                        cry("Redirect found before trigger", filename, lineno);
                        continue;
                    }

                    // Add the condition to the trigger.
                    topics.topic(topic).trigger(onTrig).addCondition(line);
                }
                else
                {
                    cry("Unrecognized command \"" + cmd + "\"", filename, lineno);
                }
            }

            return true;
        }

        #endregion

        #region Sorting Methods

        /// <summary>
        /// After loading replies into memory, call this method to (re)initialize
        /// internal sort buffers. This is necessary for accurate trigger matching.
        /// </summary>
        public void sortReplies()
        {
            // We need to make sort buffers under each topic.
            var topics = this.topics.listTopics();
            say("There are " + topics.Length + " topics to sort replies for.");

            // Tell the topic manager to sort its topics' replies.
            this.topics.sortReplies();

            // Sort the substitutions.
            subs_s = Util.SortByLengthDesc(subs.Keys.ToArray());
            person_s = Util.SortByLengthDesc(person.Keys.ToArray());
        }

        #endregion

        #region Reply Methods

        //TODO

        #endregion

        #region Developer Methods

        /// <summary>
        /// DEVELOPER: Dump the trigger sort buffers to the terminal.
        /// </summary>
        public void dumpSorted()
        {
            var topics = this.topics.listTopics();
            for (int t = 0; t < topics.Length; t++)
            {
                var topic = topics[t];
                var triggers = this.topics.topic(topic).listTriggers();

                // Dump.
                println("Topic: " + topic);
                for (int i = 0; i < triggers.Length; i++)
                {
                    println("       " + triggers[i]);
                }
            }
        }

        /// <summary>
        /// DEVELOPER: Dump the entire topic/trigger/reply structure to the terminal.
        /// </summary>
        public void dumpTopics()
        {
            // Dump the topic list.
            println("{");
            var topicList = topics.listTopics();
            for (int t = 0; t < topicList.Length; t++)
            {
                var topic = topicList[t];
                var extra = "";

                // Includes? Inherits?
                var includes = topics.topic(topic).listIncludes();
                var inherits = topics.topic(topic).listInherits();
                if (includes.Length > 0)
                {
                    extra = "includes ";
                    for (int i = 0; i < includes.Length; i++)
                    {
                        extra += includes[i] + " ";
                    }
                }
                if (inherits.Length > 0)
                {
                    extra += "inherits ";
                    for (int i = 0; i < inherits.Length; i++)
                    {
                        extra += inherits[i] + " ";
                    }
                }
                println("  '" + topic + "' " + extra + " => {");

                // Dump the trigger list.
                var trigList = topics.topic(topic).listTriggers();
                for (int i = 0; i < trigList.Length; i++)
                {
                    var trig = trigList[i];
                    println("    '" + trig + "' => {");

                    // Dump the replies.
                    var reply = topics.topic(topic).trigger(trig).Replies;
                    if (reply.Length > 0)
                    {
                        println("      'reply' => [");
                        for (int r = 0; r < reply.Length; r++)
                        {
                            println("        '" + reply[r] + "',");
                        }
                        println("      ],");
                    }

                    // Dump the conditions.
                    string[] cond = topics.topic(topic).trigger(trig).listConditions();
                    if (cond.Length > 0)
                    {
                        println("      'condition' => [");
                        for (int r = 0; r < cond.Length; r++)
                        {
                            println("        '" + cond[r] + "',");
                        }
                        println("      ],");
                    }

                    // Dump the redirects.
                    var red = topics.topic(topic).trigger(trig).listRedirects();
                    if (red.Length > 0)
                    {
                        println("      'redirect' => [");
                        for (int r = 0; r < red.Length; r++)
                        {
                            println("        '" + red[r] + "',");
                        }
                        println("      ],");
                    }

                    println("    },");
                }

                println("  },");
            }
        }

        #endregion

        #region Debug Methods

        protected void println(string line)
        {
            Console.WriteLine(line);
        }

        /// <summary>
        /// Print a line of debug text to the terminal.
        /// </summary>
        /// <param name="line"></param>
        protected void say(string line)
        {
            if (debug)
            {
                Console.WriteLine("[RS] " + line);
            }
        }

        /// <summary>
        /// Print a line of warning text to the terminal.
        /// </summary>
        /// <param name="line"></param>
        protected void cry(string line)
        {
            Console.WriteLine("<RS> " + line);
        }

        /// <summary>
        /// Print a line of warning text including a file name and line number.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        protected void cry(string text, string file, int line)
        {
            Console.WriteLine("<RS> " + text + " at " + file + " line " + line + ".");
        }

        /// <summary>
        /// Print a stack trace to the terminal when debug mode is on.
        /// </summary>
        /// <param name="e"></param>
        protected void trace(System.IO.IOException e)
        {
            if (this.debug)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        #endregion
    }
}
