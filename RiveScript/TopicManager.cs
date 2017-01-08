using System;
using System.Collections.Generic;

namespace RiveScript
{
    /// <summary>
    /// Topic manager class for RiveScript.
    /// </summary>
    public class TopicManager
    {
        private Dictionary<string, Topic> topics = new Dictionary<string, Topic>(); // Hash of managed topics
        private ICollection<string> lTopics = new List<string>(); // A vector of topics

        /// <summary>
        /// Create a topic manager. Only one per RiveScript interpreter needed.
        /// </summary>
        public TopicManager() { }

        /// <summary>
        /// Specify which topic any following operations will operate under.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Topic topic(string topic)
        {
            if (false == topics.ContainsKey(topic))
            {
                topics.Add(topic, new Topic(topic));
                lTopics.Add(topic);
            }

            return topics[topic];
        }

        /// <summary>
        /// Test whether a topic exists.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool exists(string topic)
        {
            return topics.ContainsKey(topic);
        }

        /// <summary>
        ///  Retrieve a list of the existing topics.
        /// </summary>
        public string[] listTopics()
        {
            return lTopics.ToArray();
        }

        /// <summary>
        /// Sort the replies in all the topics.This will build trigger lists of
        /// the topics(taking into account topic inheritence/includes) and sending
        /// the final trigger list into each topic's individual sortTriggers() method.
        /// </summary>
        public void sortReplies()
        {
            foreach (var topic in this.listTopics())
            {

                var allTrig = this.topicTriggers(topic, 0, 0, false);

                // Make this topic sort using this trigger list.
                this.topic(topic).sortTriggers(allTrig);

                // Make the topic update its %Previous buffer.
                this.topic(topic).sortPrevious();
            }
        }

        /// <summary>
        /// Walk the inherit/include trees and return a list of unsorted triggers.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="depth"></param>
        /// <param name="inheritance"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        private string[] topicTriggers(string topic, int depth, int inheritance, bool inherited)
        {
            // Break if we're too deep.
            if (depth > 50)
            {
                Console.WriteLine("[ERROR] Deep recursion while scanning topic inheritance (topic " + topic + " was involved)");
                return new string[0];
            }
            /*
			Important info about the depth vs inheritance params to this function:
			depth increments by 1 every time this function recursively calls itself.
			inheritance increments by 1 only when this topic inherits another topic.

			This way, '>topic alpha includes beta inherits gamma' will have this effect:
				alpha and beta's triggers are combined together into one pool, and then
				these triggers have higher matching priority than gamma's.

			The inherited option is true if this is a recursive call, from a topic
			that inherits other topics. This forces the {inherits} tag to be added to
			the triggers, for the topic's sortTriggers() to deal with. This only applies
			when the top topic "includes" another topic.
		    */


            // Collect an array of triggers to return.
            var triggers = new List<string>();

            // Does this topic include others?
            var includes = this.topic(topic).includes();
            if (includes.Length > 0)
            {
                for (int i = 0; i < includes.Length; i++)
                {
                    // Recurse.
                    var recursive = this.topicTriggers(includes[i], (depth + 1), inheritance, false);
                    for (int j = 0; j < recursive.Length; j++)
                    {
                        triggers.Add(recursive[j]);
                    }
                }
            }

            // Does this topic inherit others?
            var inherits = this.topic(topic).inherits();
            if (inherits.Length > 0)
            {
                for (int i = 0; i < inherits.Length; i++)
                {
                    // Recurse.
                    var recursive = this.topicTriggers(inherits[i], (depth + 1), (inheritance + 1), true);
                    for (int j = 0; j < recursive.Length; j++)
                    {
                        triggers.Add(recursive[j]);
                    }
                }
            }

            // Collect the triggers for *this* topic. If this topic inherits any other
            // topics, it means that this topic's triggers have higher priority than
            // those in any inherited topics. Enforce this with an {inherits} tag.
            var localTriggers = this.topic(topic).listTriggers(true);
            if (inherits.Length > 0 || inherited)
            {
                // Get the raw unsorted triggers.
                for (int i = 0; i < localTriggers.Length; i++)
                {
                    // Skip any trigger with a {previous} tag, these are for %Previous
                    // and don't go in the general population.
                    if (localTriggers[i].IndexOf("{previous}") > -1)
                    {
                        continue;
                    }

                    // Prefix it with an {inherits} tag.
                    triggers.Add("{inherits=" + inheritance + "}" + localTriggers[i]);
                }
            }
            else
            {
                // No need for an inherits tag here.
                for (int i = 0; i < localTriggers.Length; i++)
                {
                    // Skip any trigger with a {previous} tag, these are for %Previous
                    // and don't go in the general population.
                    if (localTriggers[i].IndexOf("{previous}") > -1)
                    {
                        continue;
                    }

                    triggers.Add(localTriggers[i]);
                }
            }

            // Return it as an array.
            return triggers.ToArray();
        }

        /// <summary>
        /// Walk the inherit/include trees starting with one topic and find the trigger
        /// object that corresponds to the search trigger.Or rather, if you have a trigger
        /// that was part of a topic's sort list, but that topic itself doesn't manage
        /// that trigger, this function will search the tree to find the topic that does,
        /// and return its Trigger object.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="pattern"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public Trigger findTriggerByInheritance(string topic, string pattern, int depth)
        {
            // Break if we're too deep.
            if (depth > 50)
            {
                Console.WriteLine("[ERROR] Deep recursion while scanning topic inheritance (topic " + topic + " was involved)");
                return null;
            }

            // Inheritance is more important than inclusion.
            var inherits = this.topic(topic).inherits();
            for (int i = 0; i < inherits.Length; i++)
            {
                // Does this topic have our trigger?
                if (this.topic(inherits[i]).triggerExists(pattern))
                {
                    // Good! Return it!
                    return this.topic(inherits[i]).trigger(pattern);
                }
                else
                {
                    // Recurse.
                    var match = this.findTriggerByInheritance(inherits[i], pattern, (depth + 1));
                    if (match != null)
                    {
                        // Found it!
                        return match;
                    }
                }
            }

            // Now check for "includes".
            var includes = this.topic(topic).includes();
            for (int i = 0; i < includes.Length; i++)
            {
                // Does this topic have our trigger?
                if (this.topic(includes[i]).triggerExists(pattern))
                {
                    // Good! Return it!
                    return this.topic(includes[i]).trigger(pattern);
                }
                else
                {
                    // Recurse.
                    var match = this.findTriggerByInheritance(includes[i], pattern, (depth + 1));
                    if (match != null)
                    {
                        // Found it!
                        return match;
                    }
                }
            }

            // Don't know what else we can do.
            return null;
        }

        /// <summary>
        /// Walk the inherit/include trees starting with one topic and list every topic we find.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public string[] getTopicTree(string topic, int depth)
        {
            // Avoid deep recursion.
            if (depth >= 50)
            {
                Console.WriteLine("[ERROR] Deep recursion while scanning topic inheritance (topic " + topic + " was involved)");
                return new String[0];
            }

            // Collect a vector of topics.
            var result = new List<string>();
            result.Add(topic);

            // Does this topic include others?
            var includes = this.topic(topic).includes();
            for (int i = 0; i < includes.Length; i++)
            {
                //Recurse.
                var children = this.getTopicTree(includes[i], (depth + 1));
                for (int j = 0; j < children.Length; j++)
                {
                    result.Add(children[j]);
                }
            }

            // Does it inherit?
            var inherits = this.topic(topic).inherits();
            for (int i = 0; i < inherits.Length; i++)
            {
                //Recurse
                var children = this.getTopicTree(inherits[i], (depth + 1));
                for (int j = 0; j < children.Length; j++)
                {
                    result.Add(children[j]);
                }
            }

            // Return.
            return result.ToArray();
        }
    }
}