using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    public class RiveScript
    {
        // Private class variables.
        private bool debug = false;                 // Debug mode
        private int depth = 50;                     // Recursion depth limit
        private string error = "";                  // Last error text
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
            Topic.SetDebug(debug);
        }

        /// <summary>
        /// Create a new RiveScript interpreter object with debug disabled.
        /// </summary>
        public RiveScript() : this(false) { }

        /// <summary>
        /// Return the text of the last error message given.
        /// </summary>
        public string Error
        {
            get
            {
                return this.error;
            }
        }

        /// <summary>
        /// Set the error message.
        /// </summary>
        /// <param name="message">The new error message to set.</param>
        /// <returns></returns>
        protected bool SetError(string message)
        {
            error = message;
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
        public bool LoadDirectory(string path, string[] exts)
        {
            say("Load directory: " + path);

            // Verify Directory
            if (false == Directory.Exists(path))
                return SetError("Directory not found");

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
                return SetError("Couldn't read any files from directory " + path);
            }

            // Parse each file.
            foreach (var file in files)
            {
                LoadFile(path + "/" + file);
            }

            return true;
        }

        /// <summary>
        ///  Load a directory full of RiveScript documents (.rive or .rs files).
        /// </summary>
        /// <param name="path">The path to the directory containing RiveScript documents.</param>
        /// <returns></returns>
        public bool LoadDirectory(string path)
        {
            return LoadDirectory(path, new[] { ".rive", ".rs" });
        }

        /// <summary>
        /// Load a single RiveScript document.
        /// </summary>
        /// <param name="file">file Path to a RiveScript document.</param>
        private bool LoadFile(string file)
        {
            say("Load file: " + file);

            // Run some sanity checks on the file handle.
            if (false == File.Exists(file)) //Verify is is a file and is exists
                return SetError(file + ": file not found.");

            if (false == FileHelper.CanRead(file))
                return SetError(file + ": can't read from file.");


            // Slurp the file's contents.
            string[] lines;

            try
            {
                //QUESTION: With ou without UTF8??
                //lines = File.ReadAllLines(file);
                lines = File.ReadAllLines(file, Encoding.UTF8);
            }
            catch (FileNotFoundException e)
            {
                // How did this happen? We checked it earlier.
                return SetError(file + ": file not found exception.");
            }
            catch (IOException e)
            {
                trace(e);
                return SetError(file + ": IOException while reading.");
            }


            // Send the code to the parser.
            return Parse(file, lines);
        }

        /// <summary>
        /// Stream some RiveScript code directly into the interpreter (as a single string
        /// containing newlines in it).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Stream(string code)
        {
            // Split the given code up into lines.
            var lines = code.Split(new[] { "\n" }, StringSplitOptions.None);

            // Send the lines to the parser.
            return Parse("(streamed)", lines);
        }

        /// <summary>
        /// Stream some RiveScript code directly into the interpreter (as a string array,
        /// one line per item).
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Stream(string[] code)
        {
            // The coder has already broken the lines for us!
            return Parse("(streamed)", code);
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Add a handler for a programming language to be used with RiveScript object calls.
        /// </summary>
        /// <param name="name">The name of the programming language.</param>
        /// <param name="handler">An instance of a class that implements an ObjectHandler.</param>
        public void SetHandler(string name, IObjectHandler handler)
        {
            handlers.Add(name, handler);
        }

        /// <summary>
        /// Set a global variable for the interpreter (equivalent to ! global).
        /// Set the value to null to delete the variable.<p>
        ///
        /// There are two special globals that require certain data types:<p>
        ///
        /// debug is boolean-like and its value must be a string value containing
        /// "true", "yes", "1", "false", "no" or "0".<p>
        ///
        /// depth is integer-like and its value must be a quoted integer like "50".
        /// The "depth" variable controls how many levels deep RiveScript will go when
        /// following reply redirections.<p>
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable's value.</param>
        /// <returns></returns>
        public bool SetGlobal(string name, string value)
        {
            var delete = false;
            if (value == null || value == "<undef>")
            {
                delete = true;
            }

            // Special globals
            if (name == "debug")
            {
                // Debug is a boolean.
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
                    return SetError("Global variable \"debug\" needs a boolean value");
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
                    return SetError("Global variable \"depth\" needs an integer value");
                }
                catch (OverflowException)
                {
                    return SetError("Global variable \"depth\" is to long");
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
        public bool SetVariable(string name, string value)
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
        public bool SetSubstitution(string pattern, string output)
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
        public bool SetPersonSubstitution(string pattern, string output)
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
        public bool SetUservar(string user, string name, string value)
        {
            if (value == null || value == "<undef>")
            {
                clients.Client(user).Delete(name);
            }
            else
            {
                clients.Client(user).Set(name, value);
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
        public bool SetUservars(string user, IDictionary<string, string> data)
        {
            // TODO: this should be handled more sanely. ;)
            clients.Client(user).ReplaceData(data);
            return true;
        }

        /// <summary>
        /// Get a list of all the user IDs the bot knows about.
        /// </summary>
        public string[] Users
        {
            get
            {
                // Get the user list from the clients object.
                return clients.Clients;
            }
        }

        /// <summary>
        ///  Retrieve a listing of all the uservars for a user as a HashMap.
        /// Returns null if the user doesn't exist.
        /// </summary>
        /// <param name="user">The user ID to get the vars for.</param>
        /// <returns></returns>
        public IDictionary<string, string> GetUserVars(string user)
        {
            if (clients.Exists(user))
            {
                return clients.Client(user).Data;
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
        public string GetUserVar(string user, string name)
        {
            if (clients.Exists(user))
            {
                return clients.Client(user).Get(name);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region  Parsing Methods

        private bool Parse(string filename, string[] code)
        {
            //TODO
            throw new NotImplementedException();
        }

        #endregion

        #region Sorting Methods

        /// <summary>
        /// After loading replies into memory, call this method to (re)initialize
        /// internal sort buffers. This is necessary for accurate trigger matching.
        /// </summary>
        public void SortReplies()
        {
            // We need to make sort buffers under each topic.
            var topics = this.topics.Topics;
            say("There are " + topics.Length + " topics to sort replies for.");

            // Tell the topic manager to sort its topics' replies.
            this.topics.SortReplies();

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
            var topics = this.topics.Topics;
            for (int t = 0; t < topics.Length; t++)
            {
                var topic = topics[t];
                var triggers = this.topics.Topic(topic).ListTriggers();

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
            var topicList = topics.Topics;
            for (int t = 0; t < topicList.Length; t++)
            {
                var topic = topicList[t];
                var extra = "";

                // Includes? Inherits?
                var includes = topics.Topic(topic).ListIncludes();
                var inherits = topics.Topic(topic).ListInherits();
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
                var trigList = topics.Topic(topic).ListTriggers();
                for (int i = 0; i < trigList.Length; i++)
                {
                    var trig = trigList[i];
                    println("    '" + trig + "' => {");

                    // Dump the replies.
                    var reply = topics.Topic(topic).Trigger(trig).Replies;
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
                    String[] cond = topics.Topic(topic).Trigger(trig).Conditions;
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
                    var red = topics.Topic(topic).Trigger(trig).Redirects;
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
        protected void say(String line)
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
        protected void cry(String line)
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
