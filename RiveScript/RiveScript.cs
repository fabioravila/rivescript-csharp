using RiveScript.AST;
using RiveScript.Log;
using RiveScript.Macro;
using RiveScript.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using RiveScript.Parse;
using RiveScript.Lang;

namespace RiveScript
{
    public class RiveScript
    {
        static readonly string[] DEFAULT_FILE_EXTENSIONS = new[] { ".rive", ".rs" };
        static Random rand = new Random();  // A random number generator

        // Private class variables.
        bool debug = false;                 // Debug mode
        int depth = 50;                     // Recursion depth limit
        bool utf8 = false;                  // uft8 mode flag
        bool strict = true;                 // strict mode flag
        bool forceCase = false;
        Regex unicodePunctuation;
        Action<string> onDebug = null;
        string _error = "";                  // Last error text

        //Logger
        ILogger logger;

        //user variable session manager
        // Bot's users' data structure.
        ISessionManager sessions;

        CSharp csharpHandler;

        //THe AST parser
        Parser parser;

        // The topic data structure, and the "thats" data structure.
        TopicManager topics;

        // Object handlers //TODO: At this moment, just have a CSHarpHander
        IDictionary<string, IObjectHandler> handlers = new Dictionary<string, IObjectHandler>();
        IDictionary<string, string> objectLanguages = new Dictionary<string, string>(); // name->language mappers
        IDictionary<string, ISubroutine> subroutines;        // Java object handlers

        // Simpler internal data structures.
        ICollection<string> listTopics = new List<string>();                                                // vector containing topic list (for quicker lookups)
        IDictionary<string, string> globals = new Dictionary<string, string>();                             // ! global
        IDictionary<string, string> vars = new Dictionary<string, string>();                                // ! var
        IDictionary<string, ICollection<string>> arrays = new Dictionary<string, ICollection<string>>();    // ! array
        IDictionary<string, string> subs = new Dictionary<string, string>();                                // ! sub
        IDictionary<string, string> person = new Dictionary<string, string>();                              // ! person


        string[] person_s = null; // sorted persons
        string[] subs_s = null;                                                                             // sorted subs
        ThreadLocal<string> _currentUser = new ThreadLocal<string>();
        public ErrorMessages errors { get; private set; }

        public bool IsErrReply(string reply)
        {
            var test = (reply ?? "").Trim();

            return test == errors.deepRecursion ||
                   test == errors.objectNotFound ||
                   test == errors.replyNotFound ||
                   test == errors.replyNotMatched;
        }

        /// <summary>
        /// Create a new RiveScript interpreter object with default options
        /// </summary>
        public RiveScript() : this(null) { }

        /// <summary>
        /// Create a new RiveScript interpreter object with default options
        /// </summary>
        /// <param name="config">Options object</param>
        public RiveScript(Config config)
        {
            if (config == null)
                config = Config.Default;

            logger = config.logger ?? new EmptyLogger();
            Topic.setLogger(logger);

            sessions = config.sessionManager ?? new ConcurrentDictionarySessionManager();

            debug = config.debug;
            utf8 = config.utf8;
            strict = config.strict;
            _currentUser.Value = Constants.Undefined;
            forceCase = config.forceCase;
            onDebug = config.onDebug;
            unicodePunctuation = new Regex($"{config.unicodePonctuations}/g");
            depth = config.depth;

            if (depth <= 0) depth = Config.DEFAULT_DEPTH;//Adjust depth

            //Errors
            errors = (config.errors ?? ErrorMessages.Default).AdjustDefaults();

            //Create a TOpicManager, must be single for rivescript instance
            topics = new TopicManager();

            //Create parser
            parser = new Parser(new ParserConfig(strict: strict, utf8: utf8, forceCase: forceCase), topics);

            csharpHandler = new CSharp(this);
            //CSharp handler is default
            handlers.Add(Constants.CSharpHandlerName, csharpHandler);
        }

        public void setDebug(bool debug)
        {
            this.debug = debug;
            //Topic.setDebug(debug);
        }


        public string getVersion()
        {
            return typeof(RiveScript).Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Return the text of the last error message given.
        /// </summary>
        public string error()
        {
            return _error;
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
        /// Load a single RiveScript document.
        /// </summary>
        /// <param name="file">file Path to a RiveScript document.</param>
        public bool loadFile(string file)
        {
            logger.Debug("Load file: " + file);

            // Run some sanity checks on the file handle.
            if (!File.Exists(file)) //Verify is is a file and is exists
                return error(file + ": file not found.");

            if (!FileHelper.CanRead(file))
                return error(file + ": can't read from file.");


            // Slurp the file's contents.
            string[] lines;

            try
            {
                //QUESTION: With ou without UTF8??
                var encode = FileHelper.GetEncoding(file);
                lines = File.ReadAllLines(file, encode);
            }
            catch (FileNotFoundException)
            {
                // How did this happen? We checked it earlier.
                return error(file + ": file not found exception.");
            }
            catch (IOException e)
            {
                logger.Error(e);
                return error(file + ": IOException while reading.");
            }


            // Send the code to the parser.
            return parse(file, lines);
        }

        /// <summary>
        /// Load a directory full of RiveScript documents, specifying a custom
	    /// list of valid file extensions.
        /// </summary>
        /// <param name="path">The path to the directory containing RiveScript documents.</param>
        /// <param name="exts">A string array containing file extensions to look for.</param>
        /// <returns></returns>
        public bool loadDirectory(string path, string[] exts)
        {
            logger.Debug("Load directory: " + path);

            // Verify Directory
            if (!Directory.Exists(path))
                return error("Directory not found");

            if (exts == null || exts.Length == 0)
                exts = DEFAULT_FILE_EXTENSIONS;

            //Adjust exts for all have .
            for (int i = 0; i < exts.Length; i++)
            {
                if (!exts[i].StartsWith("."))
                {
                    exts[i] = "." + exts[i];
                }
            }

            //TODO: I can Do better - I try avoid Linq

            // Search it for files recursive
            var files = new List<string>();
            foreach (var f in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (Array.IndexOf(exts, Path.GetExtension(f)) > -1)
                {
                    files.Add(f);
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
                loadFile(file);
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
            return loadDirectory(path, DEFAULT_FILE_EXTENSIONS);
        }

        /// <summary>
        /// Stream some RiveScript code directly into the interpreter (as a single string
        /// containing newlines in it).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool stream(string code)
        {
            //Adjust carriage return
            code = code.Replace("\r\n", "\n");

            // Split the given code up into lines.
            var lines = code.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

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
            //Do not modify C# handler (for now)
            if (name == Constants.CSharpHandlerName)
                return;

            handlers.Add(name, handler);
        }

        /// <summary>
        /// Remove a handler for a programming language to be used with RiveScript object calls.
        /// </summary>
        /// <param name="name">The name of the programming language.</param>
        public void removeHandler(string name)
        {
            //Do not remove C# handler, becou we will use this to agregate all subrotines and avois conflicts
            if (name == Constants.CSharpHandlerName)
                return;

            handlers.Remove(name);
        }

        /// <summary>
        /// If desire, can put a macro concrete compiled implementation in C#
        /// </summary>
        /// <param name="name">name of the </param>
        /// <param name="subroutine"></param>
        public void setSubroutine(string name, ISubroutine subroutine)
        {
            csharpHandler.AddSubroutine(name, subroutine);
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
            if (value == null || value == Constants.UNDEF_TAG)
            {
                delete = true;
            }

            // Special globals
            if (name == "debug")
            {
                // Debug is a bool.
                if (Util.IsTrue(value))
                {
                    debug = true;
                }
                else if (Util.IsFalse(value) || delete)
                {
                    debug = false;
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
                    depth = int.Parse(value);
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
                globals.AddOrUpdate(name, value);
            }

            return true;
        }

        /// <summary>
        /// This is equivalent to <env> in RiveScript. Returns null if the variable isn't defined.
        /// </summary>
        /// <param name="name">name the variable name</param>
        /// <returns></returns>
        public string getGlobal(string name)
        {
            if (name == "depth")
                return depth.ToString();

            return globals.GetOrDefault(name);
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
            if (value == null || value == Constants.UNDEF_TAG)
                vars.Remove(name);
            else
                vars.AddOrUpdate(name, value);

            return true;
        }

        /// <summary>
        /// This is equivalent to <bot> in RiveScript. Returns null if the variable isn't defined.
        /// </summary>
        /// <param name="name">name the variable name</param>
        /// <returns></returns>
        public string getVariable(string name)
        {
            return vars.GetOrDefault(name);
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
            if (output == null || output == Constants.UNDEF_TAG)
                subs.Remove(pattern);
            else
                subs.AddOrUpdate(pattern, output);

            return true;
        }

        public bool setArray(string name, ICollection<string> value)
        {
            //if (value == null || value.Count == 0 || value[0] == Constants.UNDEF_TAG)
            if (value == null || value.Count == 0)
                arrays.Remove(name);
            else
                arrays.AddOrUpdate(name, value);

            return true;
        }

        /// <summary>
        /// This is equivalent to <sub> in RiveScript. Returns null if the variable isn't defined.
        /// </summary>
        /// <param name="pattern">pattern the variable name</param>
        /// <returns></returns>
        public string getSubstitution(string pattern)
        {
            return subs.GetOrDefault(pattern);
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
            if (output == null || output == Constants.UNDEF_TAG)
                person.Remove(pattern);
            else
                person.AddOrUpdate(pattern, output);

            return true;
        }

        /// <summary>
        /// This is equivalent to <person> in RiveScript. Returns null if the variable isn't defined.
        /// </summary>
        /// <param name="pattern">pattern the variable name</param>
        /// <returns></returns>
        public string getPersonSubstitution(string pattern)
        {
            return person.GetOrDefault(pattern);
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
            if (value == null || value == Constants.UNDEF_TAG)
            {
                sessions.remove(user, name);
            }
            else
            {
                // Topic? And are we forcing case?
                if (name == "topic" && forceCase)
                    value = value.ToLower();

                sessions.set(user, name, value);
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
            sessions.set(user, data);
            return true;
        }



        /// <summary>
        /// Get a list of all the user IDs the bot knows about.
        /// </summary>
        public string[] getUsers()
        {
            // Get the user list from the clients object.
            return sessions.getAll().Keys.ToArray();
        }

        /// <summary>
        /// Return the sessions manager instance
        /// </summary>
        /// <returns>current session manager instance</returns>
        public ISessionManager getSessionManager() => sessions;

        /// <summary>
        ///  Retrieve a listing of all the uservars for a user as a HashMap.
        /// Returns null if the user doesn't exist.
        /// </summary>
        /// <param name="user">The user ID to get the vars for.</param>
        /// <returns></returns>
        public IDictionary<string, string> getUserVars(string user)
        {
            return sessions.get(user).getVariables();
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
            return sessions.get(user, name);
        }

        public string currentUser()
        {
            return _currentUser.Value ?? Constants.Undefined;
        }

        #endregion

        #region  Parsing Methods

        bool parse(string filename, string[] code)
        {
            var root = parser.parse(filename, code);

            //Passing all ast variables to rivescript class
            //globals
            foreach (var item in root.begin.globals)
            {
                setGlobal(item.Key, item.Value);
            }

            //vars
            foreach (var item in root.begin.vars)
            {
                setVariable(item.Key, item.Value);
            }

            //sub
            foreach (var item in root.begin.subs)
            {
                setSubstitution(item.Key, item.Value);
            }

            //person
            foreach (var item in root.begin.person)
            {
                setPersonSubstitution(item.Key, item.Value);
            }

            //array
            foreach (var item in root.begin.arrays)
            {
                setArray(item.Key, item.Value);
            }

            //objects macro
            foreach (var item in root.objects)
            {
                if (handlers.ContainsKey(item.Language))
                {
                    handlers[item.Language].Load(item.Name, item.Code.ToArray());
                    objectLanguages.Add(item.Name, item.Language);
                }
                else
                {
                    logger.Warn($"Object '{item.Name}' not loaded as no handler was found for programming language '{item.Language}'");
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
            var topics = this.topics.listTopicsName();
            logger.Debug("There are " + topics.Length + " topics to sort replies for.");

            // Tell the topic manager to sort its topics' replies.
            this.topics.sortReplies();

            // Sort the substitutions.
            subs_s = Util.SortByLengthDesc(subs.Keys.ToArray());
            person_s = Util.SortByLengthDesc(person.Keys.ToArray());
        }

        #endregion

        #region Reply Methods


        /// <summary>
        /// Get a reply from the RiveScript interpreter.
        /// </summary>
        /// <param name="username"> A unique user ID for the user chatting with the bot.</param>
        /// <param name="message">The user's message to the bot.</param>
        /// <returns></returns>
        public string reply(string username, string message)
        {
            logger.Debug("Asked reply to [" + username + "] " + message);

            var startTime = DateTime.Now.Ticks;

            _currentUser.Value = username;

            try
            {
                // Initialize a user profile for this user?
                sessions.init(username);

                // Format their message first.
                message = formatMessage(message);

                // This will hold the final reply.
                string reply = "";

                // If the BEGIN statement exists, consult it first.
                if (topics.exists("__begin__"))
                {
                    string begin = getReply(username, "request", true, 0);

                    // OK to continue? // Get a reply then.
                    if (begin.IndexOf("{ok}") > -1)
                    {
                        reply = getReply(username, message, false, 0);
                        begin = begin.ReplaceRegex("\\{ok\\}", reply);
                    }


                    reply = begin;
                    // Run final substitutions.
                    reply = processTags(username, sessions.get(username), message, reply,
                        new List<string>(), new List<string>(),
                        0);
                }
                else
                {
                    // No BEGIN, just continue.
                    reply = getReply(username, message, false, 0);
                }

                // Save their chat history.
                sessions.addHistory(username, message, reply);
                _currentUser.Value = Constants.Undefined;

                // Return their reply.
                return reply;
            }
            finally
            {

            }
        }

        /// <summary>
        ///  Internal method for getting a reply.
        /// </summary>
        /// <param name="user">The username of the calling user.</param>
        /// <param name="message">The (formatted!) message sent by the user.</param>
        /// <param name="begin">Whether the context is that we're in the BEGIN statement or not.</param>
        /// <param name="step">The recursion depth that we're at so far.</param>
        /// <returns></returns>
        string getReply(string user, string message, bool begin, int step)
        {
            /*-----------------------*/
            /*-- Collect User Info --*/
            /*-----------------------*/

            var topic = "random";             // Default topic = random
            var stars = new List<string>(); // Wildcard matches
            var botstars = new List<string>(); // Wildcards in %Previous
            var _reply = "";                   // The eventual reply
            UserData profile = null;                  // The user's profile object
            // Get the user's profile.
            profile = sessions.get(user);

            // Update their topic.
            topic = profile.getVariable("topic");

            // Avoid letting the user fall into a missing topic.
            if (topics.exists(topic) == false)
            {
                logger.Warn("User " + user + " was in a missing topic named \"" + topic + "\"!");
                topic = "random";
                profile.setVariable("topic", "random");
            }

            // Avoid deep recursion.
            if (step > depth)
            {
                _reply = errors.deepRecursion;
                logger.Warn(_reply);
                return _reply;
            }

            // Are we in the BEGIN statement?
            if (begin)
            {
                // This implies the begin topic.
                topic = "__begin__";
            }

            /*------------------*/
            /*-- Find a Reply --*/
            /*------------------*/

            // Create a pointer for the matched data.
            Trigger matched = null;
            bool foundMatch = false;
            string matchedTrigger = "";

            // See if there are any %previous's in this topic, or any topic related to it. This
            // should only be done the first time -- not during a recursive redirection.
            // This is because in a redirection, "lastreply" is still gonna
            // be the same as it was the first time, resulting in an infinite loop!
            if (step == 0)
            {
                logger.Debug("Looking for a %Previous");
                string[] allTopics = { topic };

                if (topics.topic(topic).includes().Length > 0 || topics.topic(topic).inherits().Length > 0)
                {
                    // We need to walk the topic tree.
                    allTopics = topics.getTopicTree(topic, 0);
                }


                for (int i = 0; i < allTopics.Length; i++)
                {
                    // Does this topic have a %Previous anywhere?
                    logger.Debug("Seeing if " + allTopics[i] + " has a %Previous");
                    if (topics.topic(allTopics[i]).hasPrevious())
                    {
                        logger.Debug("Topic " + allTopics[i] + " has at least one %Previous");

                        // Get them.
                        string[] previous = topics.topic(allTopics[i]).listPrevious();
                        for (int j = 0; j < previous.Length; j++)
                        {
                            logger.Debug("Candidate: " + previous[j]);

                            // Try to match the bot's last reply against this.
                            string lastReply = formatMessage(profile.history.getReply(1), true);
                            string regexp = triggerRegexp(user, profile, previous[j]);
                            logger.Debug("Compare " + lastReply + " <=> " + previous[j] + " (" + regexp + ")");

                            // Does it match?
                            Regex re = new Regex("^" + regexp + "$");
                            foreach (Match m in re.Matches(lastReply))
                            {
                                logger.Debug("OMFG the lastReply matches!");

                                // Harvest the botstars.
                                for (int s = 1; s <= m.Groups.Count; s++)
                                {
                                    logger.Debug("Add botstar: " + m.Groups[s].Value);
                                    botstars.Add(m.Groups[s].Value);
                                }

                                // Now see if the user matched this trigger too!
                                string[] candidates = topics.topic(allTopics[i]).listPreviousTriggers(previous[j]);
                                for (int k = 0; k < candidates.Length; k++)
                                {
                                    logger.Debug("Does the user's message match " + candidates[k] + "?");
                                    string humanside = triggerRegexp(user, profile, candidates[k]);
                                    logger.Debug("Compare " + message + " <=> " + candidates[k] + " (" + humanside + ")");

                                    Regex reH = new Regex("^" + humanside + "$");
                                    foreach (Match mH in reH.Matches(message))
                                    {
                                        logger.Debug("It's a match!!!");

                                        // Make sure it's all valid.
                                        string realTrigger = candidates[k] + "{previous}" + previous[j];
                                        if (topics.topic(allTopics[i]).triggerExists(realTrigger))
                                        {
                                            // Seems to be! Collect the stars.
                                            for (int s = 1; s <= mH.Groups.Count; s++)
                                            {
                                                var star = mH.Groups[s].Value;
                                                if (star == null)
                                                    star = "";

                                                logger.Debug("Add star: " + star);
                                                stars.Add(star);
                                            }

                                            foundMatch = true;
                                            matchedTrigger = candidates[k];
                                            matched = topics.topic(allTopics[i]).trigger(realTrigger);
                                        }

                                        break;
                                    }

                                    if (foundMatch)
                                    {
                                        break;
                                    }
                                }
                                if (foundMatch)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Search their topic for a match to their trigger.
            if (!foundMatch)
            {
                // Go through the sort buffer for their topic.
                string[] triggers = topics.topic(topic).listTriggers();
                for (int a = 0; a < triggers.Length; a++)
                {
                    string trigger = triggers[a];

                    // Prepare the trigger for the regular expression engine.
                    string regexp = triggerRegexp(user, profile, trigger);
                    logger.Debug("Try to match \"" + message + "\" against \"" + trigger + "\" (" + regexp + ")");

                    // Is it a match?
                    Regex re = new Regex("^" + regexp + "$");
                    foreach (Match m in re.Matches(message))
                    {
                        logger.Debug("The trigger matches! Star count: " + m.Groups.Count);

                        // Harvest the stars.
                        int starcount = m.Groups.Count;
                        for (int s = 1; s <= starcount; s++)
                        {
                            string star = (m.Groups[s].Value ?? "");
                            logger.Debug("Add star: " + star);
                            stars.Add(star);
                        }

                        // We found a match, but what if the trigger we matched belongs to
                        // an inherited topic? Check for that.
                        if (topics.topic(topic).triggerExists(trigger))
                        {
                            // No, the trigger does belong to us.
                            matched = topics.topic(topic).trigger(trigger);
                        }
                        else
                        {
                            logger.Debug("Trigger doesn't exist under this topic, trying to find it!");
                            matched = topics.findTriggerByInheritance(topic, trigger, 0);
                        }

                        foundMatch = true;
                        matchedTrigger = trigger;
                        break;
                    }

                    if (foundMatch)
                    {
                        break;
                    }
                }
            }

            // Store what trigger they matched on (matchedTrigger can be blank if they didn't match).
            profile.setVariable("__lastmatch__", matchedTrigger);

            // Did they match anything?
            if (foundMatch)
            {
                logger.Debug("They were successfully matched to a trigger!");

                /*---------------------------------*/
                /*-- Process Their Matched Reply --*/
                /*---------------------------------*/

                // Make a dummy once loop so we can break out anytime.
                for (int n = 0; n < 1; n++)
                {
                    // Exists?
                    if (matched == null)
                    {
                        logger.Warn("Unknown error: they matched trigger " + matchedTrigger + ", but it doesn't exist?");
                        foundMatch = false;
                        break;
                    }

                    // Get the trigger object.
                    Trigger trigger = matched;
                    logger.Debug("The trigger matched belongs to topic " + trigger.getTopic());

                    // Check for conditions.
                    string[] conditions = trigger.listConditions();
                    if (conditions.Length > 0)
                    {
                        logger.Debug("This trigger has some conditions!");

                        // See if any conditions are true.
                        bool truth = false;
                        for (int c = 0; c < conditions.Length; c++)
                        {
                            // Separate the condition from the potential reply.
                            string[] halves = conditions[c].SplitRegex("\\s*=>\\s*");
                            string condition = halves[0].Trim();
                            string potreply = halves[1].Trim();

                            // Split up the condition.
                            Regex reCond = new Regex("^(.+?)\\s+(==|eq|\\!=|ne|<>|<|<=|>|>=)\\s+(.+?)$");
                            foreach (Match mCond in reCond.Matches(condition))
                            {
                                string left = mCond.Groups[1].Value.Trim();
                                string eq = mCond.Groups[2].Value.Trim();
                                string right = mCond.Groups[3].Value.Trim();

                                // Process tags on both halves.
                                left = processTags(user, profile, message, left, stars, botstars, step + 1);
                                right = processTags(user, profile, message, right, stars, botstars, step + 1);
                                logger.Debug("Compare: " + left + " " + eq + " " + right);

                                // Defaults
                                if (left.Length == 0)
                                {
                                    left = "undefined";
                                }
                                if (right.Length == 0)
                                {
                                    right = "undefined";
                                }

                                // Validate the expression.
                                if (eq.Equals("eq") || eq.Equals("ne") || eq.Equals("==") || eq.Equals("!=") || eq.Equals("<>"))
                                {
                                    // string equality comparing.
                                    if ((eq.Equals("eq") || eq.Equals("==")) && left.Equals(right))
                                    {
                                        truth = true;
                                        break;
                                    }
                                    else if ((eq.Equals("ne") || eq.Equals("!=") || eq.Equals("<>")) && !left.Equals(right))
                                    {
                                        truth = true;
                                        break;
                                    }
                                }

                                // Numeric comparing.
                                int lt = 0;
                                int rt = 0;

                                // Turn the two sides into numbers.
                                try
                                {
                                    lt = int.Parse(left);
                                    rt = int.Parse(right);
                                }
                                catch (FormatException)
                                {
                                    // Oh well!
                                    break;
                                }

                                // Run the remaining equality checks.
                                if (eq.Equals("==") || eq.Equals("!=") || eq.Equals("<>"))
                                {
                                    // Equality checks.
                                    if (eq.Equals("==") && lt == rt)
                                    {
                                        truth = true;
                                        break;
                                    }
                                    else if ((eq.Equals("!=") || eq.Equals("<>")) && lt != rt)
                                    {
                                        truth = true;
                                        break;
                                    }
                                }
                                else if (eq.Equals("<") && lt < rt)
                                {
                                    truth = true;
                                    break;
                                }
                                else if (eq.Equals("<=") && lt <= rt)
                                {
                                    truth = true;
                                    break;
                                }
                                else if (eq.Equals(">") && lt > rt)
                                {
                                    truth = true;
                                    break;
                                }
                                else if (eq.Equals(">=") && lt >= rt)
                                {
                                    truth = true;
                                    break;
                                }
                            }

                            // True condition?
                            if (truth)
                            {
                                _reply = potreply;
                                break;
                            }
                        }
                    }

                    // Break if we got a reply from the conditions.
                    if (_reply.Length > 0)
                    {
                        break;
                    }

                    // Return one of the replies at random. We lump any redirects in as well.
                    string[] redirects = trigger.listRedirects();
                    string[] replies = trigger.listReplies();

                    // Take into account their weights.
                    List<int> bucket = new List<int>();
                    Regex reWeight = new Regex("\\{weight=(\\d+?)\\}");

                    // Look at weights on redirects.
                    for (int i = 0; i < redirects.Length; i++)
                    {
                        if (redirects[i].IndexOf("{weight=") > -1)
                        {
                            foreach (Match mWeight in reWeight.Matches(redirects[i]))
                            {
                                int weight = int.Parse(mWeight.Groups[1].Value);

                                // Add to the bucket this many times.
                                if (weight > 1)
                                {
                                    for (int j = 0; j < weight; j++)
                                    {
                                        logger.Debug("Trigger has a redirect (weight " + weight + "): " + redirects[i]);
                                        bucket.Add(i);
                                    }
                                }
                                else
                                {
                                    logger.Debug("Trigger has a redirect (weight " + weight + "): " + redirects[i]);
                                    bucket.Add(i);
                                }

                                // Only one weight is supported.
                                break;
                            }
                        }
                        else
                        {
                            logger.Debug("Trigger has a redirect: " + redirects[i]);
                            bucket.Add(i);
                        }
                    }

                    // Look at weights on replies.
                    for (int i = 0; i < replies.Length; i++)
                    {
                        if (replies[i].IndexOf("{weight=") > -1)
                        {
                            foreach (Match mWeight in reWeight.Matches(replies[i]))
                            {
                                int weight = int.Parse(mWeight.Groups[1].Value);

                                // Add to the bucket this many times.
                                if (weight > 1)
                                {
                                    for (int j = 0; j < weight; j++)
                                    {
                                        logger.Debug("Trigger has a reply (weight " + weight + "): " + replies[i]);
                                        bucket.Add(redirects.Length + i);
                                    }
                                }
                                else
                                {
                                    logger.Debug("Trigger has a reply (weight " + weight + "): " + replies[i]);
                                    bucket.Add(redirects.Length + i);
                                }

                                // Only one weight is supported.
                                break;
                            }
                        }
                        else
                        {
                            logger.Debug("Trigger has a reply: " + replies[i]);
                            bucket.Add(redirects.Length + i);
                        }
                    }

                    // Pull a random value out.
                    int[] choices = bucket.ToArray();
                    if (choices.Length > 0)
                    {
                        int choice = choices[rand.Next(choices.Length)];
                        logger.Debug("Possible choices: " + choices.Length + "; chosen: " + choice);
                        if (choice < redirects.Length)
                        {
                            // The choice was a redirect!
                            string redirect = redirects[choice].ReplaceRegex("\\{weight=\\d+\\}", "");
                            redirect = processTags(user, profile, message, redirect, stars, botstars, step);

                            logger.Debug("Chosen a redirect to " + redirect + "!");
                            _reply = getReply(user, redirect, begin, step + 1);
                        }
                        else
                        {
                            // The choice was a reply!
                            choice -= redirects.Length;
                            if (choice < replies.Length)
                            {
                                logger.Debug("Chosen a reply: " + replies[choice]);
                                _reply = replies[choice];
                            }
                        }
                    }
                }
            }

            // Still no reply?
            if (!foundMatch)
            {
                _reply = errors.replyNotMatched;
            }
            else if (_reply.Length == 0)
            {
                _reply = errors.replyNotFound;
            }

            logger.Debug("Final reply: " + _reply + "(begin: " + begin + ")");

            // Special tag processing for the BEGIN statement.
            if (begin)
            {
                // The BEGIN block may have {topic} or <set> tags and that's all.
                // <set> tag
                if (_reply.IndexOf("<set") > -1)
                {
                    Regex reSet = new Regex("<set (.+?)=(.+?)>");
                    foreach (Match mSet in reSet.Matches(_reply))
                    {
                        string tag = mSet.Groups[0].Value;
                        string var = mSet.Groups[1].Value;
                        string value = mSet.Groups[2].Value;

                        // Set the uservar.
                        profile.setVariable(var, value);
                        _reply = _reply.Replace(tag, "");
                    }
                }

                // {topic} tag
                if (_reply.IndexOf("{topic=") > -1)
                {
                    Regex reTopic = new Regex("\\{topic=(.+?)\\}");
                    foreach (Match mTopic in reTopic.Matches(_reply))
                    {
                        string tag = mTopic.Groups[0].Value;
                        topic = mTopic.Groups[1].Value;
                        logger.Debug("Set user's topic to: " + topic);
                        profile.setVariable("topic", topic);
                        _reply = _reply.Replace(tag, "");
                    }
                }
            }
            else
            {
                // Process tags.
                _reply = processTags(user, profile, message, _reply, stars, botstars, step);
            }

            return _reply;
        }

        /// <summary>
        /// Formats a trigger for the regular expression engine.
        /// </summary>
        /// <param name="user">The user ID of the caller.</param>
        /// <param name="profile">Client profile</param>
        /// <param name="trigger">The raw trigger text.</param>
        /// <returns></returns>
        string triggerRegexp(string user, UserData profile, string trigger)
        {
            // If the trigger is simply '*', it needs to become (.*?) so it catches the empty string.
            var regexp = trigger.ReplaceRegex("^\\*$", "<zerowidthstar>");

            // Simple regexps are simple.
            regexp = regexp.ReplaceRegex("\\*", "(.+?)");                   // *  ->  (.+?)
            regexp = regexp.ReplaceRegex("#", "(\\d+?)");                   // #  ->  (\d+?)
            regexp = regexp.ReplaceRegex("_", "(\\w+?)");                   // _  ->  ([A-Za-z ]+?)

            regexp = regexp.ReplaceRegex("\\s*\\{weight=\\d+\\}\\s*", "");  // Remove {weight} tags
            regexp = regexp.ReplaceRegex("<zerowidthstar>", "(.*?)");       // *  ->  (.*?)

            regexp = regexp.ReplaceRegex("\\|{ 2,}", "|");          //Remove empty entities
            regexp = regexp.ReplaceRegex("(\\(|\\[)\\|", "$1");     //Remove empty entities from start of alt/opts
            regexp = regexp.ReplaceRegex("\\| (\\) |\\])", "$1");   //Remove empty entities from end of alt/opts

            // Handle optionals.
            if (regexp.IndexOf("[") > -1)
            {
                Regex reOpts = new Regex("\\s*\\[(.+?)\\]\\s*");

                foreach (Match mOpts in reOpts.Matches(regexp))
                {
                    var optional = mOpts.Groups[0].Value;
                    var contents = mOpts.Groups[1].Value;

                    // Split them at the pipes.
                    string[] parts = contents.SplitRegex("\\|");

                    // Construct a regexp part.
                    StringBuilder re = new StringBuilder();
                    for (int i = 0; i < parts.Length; i++)
                    {
                        //// We want: \s*part\s*
                        //re.Append("\\s*" + parts[i] + "\\s*");

                        // See: https://github.com/aichaos/rivescript-js/commit/02f236e78c5d237cb046d2347fe704f5f70231c9
                        re.Append("(?:\\s|\\b)+" + parts[i] + "(?:\\s|\\b)+");


                        if (i < parts.Length - 1)
                        {
                            re.Append("|");
                        }
                    }
                    string pipes = re.ToString();

                    // If this optional had a star or anything in it, e.g. [*],
                    // make it non-matching.
                    //pipes = pipes.ReplaceRegex("\\(.+?\\)", "(?:.+?)");
                    //pipes = pipes.ReplaceRegex("\\(\\d+?\\)", "(?:\\\\d+?)");
                    //pipes = pipes.ReplaceRegex("\\(\\w+?\\)", "(?:\\\\w+?)");


                    pipes = pipes.ReplaceRegex("\\(\\.\\+\\?\\)", "(?:.+?)");
                    pipes = pipes.ReplaceRegex("\\(\\d\\+\\?\\)", "(?:\\\\d+?)");
                    pipes = pipes.ReplaceRegex("\\(\\w\\+\\?\\)", "(?:\\\\w+?)");

                    // Put the new text in.
                    //pipes = "(?:" + pipes + "|\\s*)";
                    pipes = "(?:" + pipes + "|(?:\\b|\\s)+)";

                    regexp = regexp.Replace(optional, pipes);

                }
            }

            // Make \w more accurate for our purposes.

            if (utf8)
            {
                regexp = regexp.Replace("\\w", "[\\p{L}]");
            }
            else
            {
                regexp = regexp.Replace("\\w", "[A-Za-z]");
            }


            // Filter in arrays.
            if (regexp.IndexOf("@") > -1)
            {
                // Match the array's name.
                Regex reArray = new Regex("\\@(.+?)\\b");

                foreach (Match mArray in reArray.Matches(regexp))
                {
                    string array = mArray.Groups[0].Value;
                    string name = mArray.Groups[1].Value;

                    // Do we have an array by this name?
                    if (arrays.ContainsKey(name))
                    {
                        string[] values = arrays[name].ToArray();
                        StringBuilder joined = new StringBuilder();

                        // Join the array.
                        for (int i = 0; i < values.Length; i++)
                        {
                            joined.Append(values[i]);
                            if (i < values.Length - 1)
                            {
                                joined.Append("|");
                            }
                        }

                        // Final contents...
                        string rep = "(?:" + joined.ToString() + ")";
                        regexp = regexp.Replace(array, rep);
                    }
                    else
                    {
                        // No array by this name.
                        regexp = regexp.Replace(array, "");
                    }
                }
            }

            // Filter in bot variables.
            if (regexp.IndexOf("<bot") > -1)
            {
                Regex reBot = new Regex("<bot (.+?)>");

                foreach (Match mBot in reBot.Matches(regexp))
                {
                    string tag = mBot.Groups[0].Value;
                    string var = mBot.Groups[1].Value;




                    // Have this?
                    if (vars.ContainsKey(var))
                    {
                        //string value = vars[var].ToLower().ReplaceRegex("[^a-z0-9 ]+", "");
                        string value = Util.StripNasties(vars[var].ToLower(), utf8);
                        regexp = regexp.Replace(tag, value);
                    }
                    else
                    {
                        regexp = regexp.Replace(tag, "undefined");
                    }
                }
            }

            // Filter in user variables.
            if (regexp.IndexOf("<get") > -1)
            {
                Regex reGet = new Regex("<get (.+?)>");

                foreach (Match mGet in reGet.Matches(regexp))
                {
                    string tag = mGet.Groups[0].Value;
                    string var = mGet.Groups[1].Value;
                    //string value = profile.getVariable(var).ToLower().ReplaceRegex("[^a-z0-9 ]+", "");
                    string value = Util.StripNasties(profile.getVariable(var).ToLower(), utf8);

                    // Have this?
                    regexp = regexp.Replace(tag, value);
                }
            }

            // Input and reply tags.
            regexp = regexp.ReplaceRegex("<input>", "<input1>");
            regexp = regexp.ReplaceRegex("<reply>", "<reply1>");

            if (regexp.IndexOf("<input") > -1)
            {
                Regex reInput = new Regex("<input([0-9])>");

                foreach (Match mInput in reInput.Matches(regexp))
                {
                    string tag = mInput.Groups[0].Value;
                    int index = int.Parse(mInput.Groups[1].Value);
                    //string text = profile.getInput(index).ToLower().ReplaceRegex("[^a-z0-9 ]+", "");
                    string text = Util.StripNasties(profile.history.getInput(index).ToLower(), utf8);
                    regexp = regexp.Replace(tag, text);
                }
            }

            if (regexp.IndexOf("<reply") > -1)
            {
                Regex reReply = new Regex("<reply([0-9])>");
                foreach (Match mReply in reReply.Matches(regexp))
                {
                    string tag = mReply.Groups[0].Value;
                    int index = int.Parse(mReply.Groups[1].Value);
                    //string text = profile.getReply(index).ToLower().ReplaceRegex("[^a-z0-9 ]+", "");
                    string text = Util.StripNasties(profile.history.getReply(index).ToLower(), utf8);
                    regexp = regexp.Replace(tag, text);
                }
            }

            return regexp;
        }

        /// <summary>
        /// Process reply tags.
        /// </summary>
        /// <param name="user">The name of the end user.</param>
        /// <param name="profile">The RiveScript client object holding the user's profile</param>
        /// <param name="message">The message sent by the user.</param>
        /// <param name="reply">The bot's original reply including tags.</param>
        /// <param name="vst"> The vector of wildcards the user's message matched.</param>
        /// <param name="vbst">The vector of wildcards in any %Previous.</param>
        /// <param name="step">The current recursion depth limit.</param>
        /// <returns></returns>
        string processTags(string user, UserData profile, string message, string reply,
                                   List<string> vst, List<string> vbst, int step)
        {
            // Pad the stars.
            var vstars = new List<string>();
            var vbotstars = new List<string>();

            vstars.Insert(0, "");
            vbotstars.Insert(0, "");
            vstars.AddRange(vst);
            vbotstars.AddRange(vbst);

            // Set a default first star.
            if (vstars.Count == 1)
            {
                vstars.Add("undefined");
            }
            if (vbotstars.Count == 1)
            {
                vbotstars.Add("undefined");
            }

            // Convert the stars into simple arrays.
            string[] stars = vstars.ToArray();
            string[] botstars = vbotstars.ToArray();

            var giveup = 0;

            //See: https://github.com/aichaos/rivescript-java/commit/0a923d6c62baeb0b47b15cb21bba8bedd30a2061
            // Turn arrays into randomized sets.
            if (reply.IndexOf("(@") > -1)
            {
                Regex reArray = new Regex("\\(@([A-Za-z0-9_]+)\\)");

                giveup = 0;
                foreach (Match mReply in reArray.Matches(reply))
                {
                    if (giveup++ > depth)
                    {
                        logger.Warn("Infinite loop looking for arrays in reply!");
                        giveup = 0;
                        break;
                    }

                    string tag = mReply.Groups[0].Value;
                    string name = mReply.Groups[1].Value;
                    string result;

                    if (arrays.ContainsKey(name))
                    {
                        string[] values = arrays[name].ToArray();
                        StringBuilder joined = new StringBuilder();
                        // Join the array.
                        for (int i = 0; i < values.Length; i++)
                        {
                            joined.Append(values[i]);
                            if (i < values.Length - 1)
                            {
                                joined.Append("|");
                            }
                        }
                        result = "{random}" + joined.ToString() + "{/random}";
                        reply = reply.Replace(tag, result);
                    }
                }
            }



            // Shortcut tags.
            reply = reply.ReplaceRegex("<person>", "{person}<star>{/person}");
            reply = reply.ReplaceRegex("<@>", "{@<star>}");
            reply = reply.ReplaceRegex("<formal>", "{formal}<star>{/formal}");
            reply = reply.ReplaceRegex("<sentence>", "{sentence}<star>{/sentence}");
            reply = reply.ReplaceRegex("<uppercase>", "{uppercase}<star>{/uppercase}");
            reply = reply.ReplaceRegex("<lowercase>", "{lowercase}<star>{/lowercase}");


            // Weight and star tags.
            reply = reply.ReplaceRegex("\\{weight=\\d+\\}", ""); // Remove {weight}s
            reply = reply.ReplaceRegex("<star>", stars[1]);
            reply = reply.ReplaceRegex("<botstar>", botstars[1]);
            for (int i = 1; i < stars.Length; i++)
            {
                reply = reply.ReplaceRegex("<star" + i + ">", stars[i]);
            }
            for (int i = 1; i < botstars.Length; i++)
            {
                reply = reply.ReplaceRegex("<botstar" + i + ">", botstars[i]);
            }
            reply = reply.ReplaceRegex("<(star|botstar)\\d+>", "");


            // Input and reply tags.
            reply = reply.ReplaceRegex("<input>", "<input1>");
            reply = reply.ReplaceRegex("<reply>", "<reply1>");
            if (reply.IndexOf("<input") > -1)
            {
                Regex reInput = new Regex("<input([0-9])>");
                foreach (Match mInput in reInput.Matches(reply))
                {
                    if (giveup++ > depth)
                    {
                        logger.Warn("Infinite loop looking for inputs!");
                        giveup = 0;
                        break;
                    }


                    string tag = mInput.Groups[0].Value;
                    int index = int.Parse(mInput.Groups[1].Value);
                    string text = Util.StripNasties(profile.history.getInput(index).ToLower(), utf8);
                    reply = reply.Replace(tag, text);
                }
            }

            if (reply.IndexOf("<reply") > -1)
            {
                Regex reReply = new Regex("<reply([0-9])>");
                foreach (Match mReply in reReply.Matches(reply))
                {
                    if (giveup++ > depth)
                    {
                        logger.Warn("Infinite loop looking for reply!");
                        giveup = 0;
                        break;
                    }

                    string tag = mReply.Groups[0].Value;
                    int index = int.Parse(mReply.Groups[1].Value);
                    //string text = profile.getReply(index).ToLower().ReplaceRegex("[^a-z0-9 ]+", "");
                    string text = Util.StripNasties(profile.history.getReply(index).ToLower(), utf8);
                    reply = reply.Replace(tag, text);
                }
            }

            reply = reply.ReplaceRegex("<id>", user);
            reply = reply.ReplaceRegex("\\\\s", " ");
            reply = reply.ReplaceRegex("\\\\n", "\n");
            reply = reply.ReplaceRegex("\\\\", "\\");
            reply = reply.ReplaceRegex("\\#", "#");

            // {random} tag
            if (reply.IndexOf("{random}") > -1)
            {
                Regex reRandom = new Regex("\\{random\\}(.+?)\\{\\/random\\}");
                foreach (Match mRandom in reRandom.Matches(reply))
                {
                    if (giveup++ > depth)
                    {
                        logger.Warn("Infinite loop looking for random tag!");
                        giveup = 0;
                        break;
                    }

                    string tag = mRandom.Groups[0].Value;
                    string[] candidates = mRandom.Groups[1].Value.SplitRegex("\\|");
                    string chosen = candidates[rand.Next(candidates.Length)];
                    reply = reply.Replace(tag, chosen);
                }
            }



            // {!stream} tag
            if (reply.IndexOf("{!") > -1)
            {
                Regex reStream = new Regex("\\{\\!(.+?)\\}");
                foreach (Match mStream in reStream.Matches(reply))
                {
                    string tag = mStream.Groups[0].Value;
                    string code = mStream.Groups[1].Value;
                    logger.Debug("Stream new code in: " + code);

                    // Stream it.
                    stream(code);
                    reply = reply.Replace(tag, "");
                }
            }


            // Person substitutions & string formatting
            if (reply.IndexOf("{person}") > -1 || reply.IndexOf("{formal}") > -1 || reply.IndexOf("{sentence}") > -1 ||
            reply.IndexOf("{uppercase}") > -1 || reply.IndexOf("{lowercase}") > -1)
            {
                string[] tags = { "person", "formal", "sentence", "uppercase", "lowercase" };
                for (int i = 0; i < tags.Length; i++)
                {
                    Regex reTag = new Regex("\\{" + tags[i] + "\\}(.+?)\\{\\/" + tags[i] + "\\}");
                    foreach (Match mTag in reTag.Matches(reply))
                    {
                        string tag = mTag.Groups[0].Value;
                        string text = mTag.Groups[1].Value;

                        if (tags[i] == "person")
                        {
                            // Run person substitutions.
                            logger.Debug("Run person substitutions: before: " + text);
                            text = substitute(text, person_s, person);
                            logger.Debug("After: " + text);
                            reply = reply.Replace(tag, text);
                        }
                        else
                        {
                            // string transform.
                            text = stringTransform(tags[i], text);
                            reply = reply.Replace(tag, text);
                        }

                    }
                }
            }


            // Handle all variable-related tags with an iterative regexp approach, to
            // allow for nesting of tags in arbitrary ways (think <set a=<get b>>)
            // Dummy out the <call> tags first, because we don't handle them right here.
            reply = reply.Replace("<call>", "{__call__}");
            reply = reply.Replace("</call>", "{/__call__}");


            while (true)
            {
                // This regexp will match a <tag> which contains no other tag inside it,
                // i.e. in the case of <set a=<get b>> it will match <get b> but not the
                // <set> tag, on the first pass. The second pass will get the <set> tag,
                // and so on.
                Regex reTag = new Regex("<([^<]+?)>");
                Match mTag = reTag.Match(reply);
                if (mTag == null || !mTag.Success)
                {
                    break; // No remaining tags!
                }


                string match = mTag.Groups[1].Value;
                string[] parts = match.Split(" ");
                string tag = parts[0].ToLower();
                string data = "";
                if (parts.Length > 1)
                {
                    //data = Util.Join(parts.ToSubArray(1, parts.Length), " ");
                    data = Util.Join(Util.CopyOfRange(parts, 1, parts.Length), " ");
                }
                string insert = "";

                // Handle the tags.
                if (tag == "bot" || tag == "env")
                {
                    // <bot> and <env> tags are similar
                    IDictionary<string, string> target = (tag == "bot") ? vars : globals;
                    if (data.IndexOf("=") > -1)
                    {
                        // Assigning a variable
                        parts = data.Split("=", 2);
                        string name = parts[0];
                        string value = parts[1];
                        logger.Debug("Set " + tag + " variable " + name + " = " + value);

                        target.AddOrUpdate(name, value);
                    }
                    else
                    {
                        // Getting a bot/env variable
                        if (target.ContainsKey(data))
                        {
                            insert = target[data];
                        }
                        else
                        {
                            insert = Constants.Undefined;
                        }
                    }
                }
                else if (tag == "set")
                {
                    // <set> user vars
                    parts = data.Split("=", 2);
                    string name = parts[0];
                    string value = parts[1];
                    logger.Debug("Set user var " + name + "=" + value);
                    // Set the uservar.
                    profile.setVariable(name, value);
                }
                else if (tag == "add" || tag == "sub" || tag == "mult" || tag == "div")
                {
                    // Math operator tags
                    parts = data.Split("=");
                    string name = parts[0];
                    int result = 0;

                    // Initialize the variable?
                    if (profile.getVariable(name) == Constants.Undefined)
                    {
                        profile.setVariable(name, "0");
                    }


                    try
                    {
                        int value = int.Parse(parts[1]);
                        try
                        {
                            result = int.Parse(profile.getVariable(name));

                            // Run the operation.
                            if (tag == "add")
                            {
                                result += value;
                            }
                            else if (tag == "sub")
                            {
                                result -= value;
                            }
                            else if (tag == "mult")
                            {
                                result *= value;
                            }
                            else
                            {
                                // Don't divide by zero.
                                if (value == 0)
                                {
                                    insert = "[ERR: Can't divide by zero!]";
                                }

                                result /= value;
                            }
                        }
                        catch (FormatException e)
                        {
                            insert = "[ERR: Math can't \"" + tag + "\" non-numeric variable " + name + "]";
                        }
                    }
                    catch (FormatException e)
                    {
                        insert = "[ERR: Math can't \"" + tag + "\" non-numeric value " + parts[1] + "]";
                    }

                    // No errors?
                    if (insert == "")
                    {
                        profile.setVariable(name, result.ToString());
                    }
                }
                else if (tag == "get")
                {
                    // Get the user var.
                    insert = profile.getVariable(data);
                }
                else
                {
                    // Unrecognized tag, preserve it
                    insert = "\\x00" + match + "\\x01";
                }

                reply = reply.Replace(mTag.Groups[0].Value, insert);
            }

            // Recover mangled HTML-like tags
            reply = reply.Replace("\\x00", "<");
            reply = reply.Replace("\\x01", ">");


            // {topic} tag
            if (reply.IndexOf("{topic=") > -1)
            {
                Regex reTopic = new Regex("\\{topic=(.+?)\\}");
                foreach (Match mTopic in reTopic.Matches(reply))
                {
                    string tag = mTopic.Groups[0].Value;
                    string topic = mTopic.Groups[1].Value;
                    logger.Debug("Set user's topic to: " + topic);
                    profile.setVariable("topic", topic);
                    reply = reply.Replace(tag, "");
                }
            }

            // {@redirect} tag
            if (reply.IndexOf("{@") > -1)
            {
                //Regex reRed = new Regex("\\{@(.+?)\\}");
                Regex reRed = new Regex("\\{@([^\\}]*?)\\}");

                foreach (Match mRed in reRed.Matches(reply))
                {
                    string tag = mRed.Groups[0].Value;
                    string target = mRed.Groups[1].Value.Trim();

                    // Do the reply redirect.
                    string subreply = this.getReply(user, target, false, step + 1);
                    reply = reply.Replace(tag, subreply);
                }
            }

            // <call> tag
            reply = reply.Replace("{__call__}", "<call>");
            reply = reply.Replace("{/__call__}", "</call>");
            if (reply.IndexOf("<call>") > -1)
            {
                Regex reCall = new Regex("<call>(.+?)<\\/call>");
                foreach (Match mCall in reCall.Matches(reply))
                {
                    var tag = mCall.Groups[0].Value;
                    var data = mCall.Groups[1].Value;
                    var parts = data.Split(" ");
                    var name = parts[0];
                    var args = new List<string>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        args.Add(parts[i]);
                    }

                    // See if we know of this object.
                    if (objectLanguages.ContainsKey(name))
                    {
                        // What language handles it?
                        string lang = objectLanguages[name];
                        string result = handlers[lang].Call(name, this, args.ToArray());
                        reply = reply.Replace(tag, result);
                    }
                    else
                    {
                        reply = reply.Replace(tag, errors.objectNotFound);
                    }
                }
            }

            return reply;
        }

        /// <summary>
        /// Reformats a string in a certain way: formal, uppercase, lowercase, sentence.
        /// </summary>
        /// <param name="format">The format you want the string in.</param>
        /// <param name="text">  The text to format.</param>
        /// <returns></returns>
        string stringTransform(string format, string text)
        {
            if (format.Equals("uppercase"))
            {
                return text.ToUpper();
            }
            else if (format.Equals("lowercase"))
            {
                return text.ToLower();
            }
            else if (format.Equals("formal"))
            {
                // Capitalize Each First Letter
                string[] words = text.Split(" ");
                logger.Debug("wc: " + words.Length);
                for (int i = 0; i < words.Length; i++)
                {
                    logger.Debug("word: " + words[i]);
                    //string[] letters = words[i].split("");
                    string[] letters = words[i].SplitRegex("");
                    logger.Debug("cc: " + letters.Length);
                    if (letters.Length > 1)
                    {
                        //Note : On splitregex, first and last are blank spaces, so use 1 index
                        letters[1] = letters[1].ToUpper();
                        words[i] = String.Join("", letters);
                        logger.Debug("new word: " + words[i]);
                    }
                }
                return String.Join(" ", words);
            }
            else if (format.Equals("sentence"))
            {
                // Uppercase the first letter of the first word.
                //Note : On splitregex, first and last are blank spaces, so use 1 index
                string[] letters = text.SplitRegex("");
                if (letters.Length > 1)
                {
                    letters[1] = letters[1].ToUpper();
                }

                return String.Join("", letters);
            }
            else
            {
                return "[ERR: Unknown string Transform " + format + "]";
            }
        }

        /// <summary>
        /// Format the user's message to begin reply matching. Lowercases it, runs substitutions,
	    /// and neutralizes what's left.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string formatMessage(string message, bool botReply = false)
        {
            // Lowercase it first.
            message = message.ToLower();

            // Run substitutions sanitize.
            message = substitute(message, subs_s, subs);

            //Trim start and end
            message = message.TrimStart();
            message = message.TrimEnd();
            message = message.Trim();


            // In UTF-8 mode, only strip metacharcters and HTML brackets (to protect against obvious XSS attacks).
            if (utf8)
            {
                message = message.ReplaceRegex("[\\<>]+", "");

                if (unicodePunctuation != null)
                {
                    message = unicodePunctuation.Replace(message, "");
                }

                // For the bot's reply, also strip common punctuation.
                if (botReply)
                {
                    message = message.ReplaceRegex("[.?,!;:@#$%^&*()]", "");
                }
            }
            else
            {
                // For everything else, strip all non-alphanumerics
                message = Util.StripNasties(message, utf8);
            }

            return message;
        }

        /// <summary>
        /// Run a substitutions on a string
        /// </summary>
        /// <param name="text">Text to apply the substitution</param>
        /// <param name="sorted"> The sorted list os substitutions patterns to process</param>
        /// <param name="hash">A hash that pairs the sorted list with the replacement texts</param>
        /// <returns></returns>
        string substitute(string text, string[] sorted, IDictionary<string, string> hash)
        {
            var ph = new List<string>();
            var pi = 0;

            for (int i = 0; i < sorted.Length; i++)
            {
                var pattern = sorted[i];
                var result = hash[sorted[i]];
                //var rot13 = Rot13.Transform(result);
                ph.Add(result);
                var placeholder = "<rot13sub>" + pi + "<bus31tor>";
                pi++;

                //Original JavaCode: var quotemeta = @pattern;
                //Run escape make sure no conflict like * in substitution
                var quotemeta = Regex.Escape(@pattern);

                text = text.ReplaceRegex(("^" + quotemeta + "$"), placeholder);
                text = text.ReplaceRegex(("^" + quotemeta + "(\\W+)"), ("" + placeholder + "$1"));
                text = text.ReplaceRegex(("(\\W+)" + quotemeta + "(\\W+)"), ("$1" + placeholder + "$2"));
                text = text.ReplaceRegex(("(\\W+)" + quotemeta + "$"), ("$1" + placeholder + ""));
            }

            var tries = 0;
            while (text.IndexOf("<rot13sub>") > -1)
            {
                tries++;
                if (tries > 50)
                {
                    Console.WriteLine("[RS] Too many loops in substitution placeholders!");
                    break;
                }

                var re = new Regex("<rot13sub>(.+?)<bus31tor>");
                var mc = re.Matches(text);
                foreach (Match m in mc)
                {
                    var block = m.Groups[0].Value;
                    var idx = int.Parse(m.Groups[1].Value);
                    text = text.Replace(block, ph[idx]);
                }
            }

            return text;
        }


        #endregion

        #region Developer Methods

        /// <summary>
        /// DEVELOPER: Dump the trigger sort buffers to the terminal.
        /// </summary>
        public void dumpSorted()
        {
            var topics = this.topics.listTopicsName();
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
            var topicList = topics.listTopicsName();
            for (int t = 0; t < topicList.Length; t++)
            {
                var topic = topicList[t];
                var extra = "";

                // Includes? Inherits?
                var includes = topics.topic(topic).includes();
                var inherits = topics.topic(topic).inherits();
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
                    var reply = topics.topic(topic).trigger(trig).listReplies();
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

        protected void println(string line)
        {
            if (debug)
                Console.WriteLine(line);
            else
                System.Diagnostics.Debug.WriteLine(line);
        }

        #endregion
    }
}
