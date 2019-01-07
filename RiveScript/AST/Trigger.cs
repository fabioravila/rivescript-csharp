using System;
using System.Collections.Generic;

namespace RiveScript.AST
{
    /// <summary>
    /// Trigger class for RiveScript.
    /// </summary>
    public class Trigger
    {
        private string pattern = "";
        private string topicName = "";
        private ICollection<string> _redirect = new List<string>();
        public string redirect { get; set; }
        private ICollection<string> reply = new List<string>();
        private ICollection<string> condition = new List<string>();
        private bool previous = false;

        public Trigger(string pattern)
        {
            this.pattern = pattern;
        }

        public string getTopic()
        {
            return topicName;
        }

        public void setTopic(string topicName)
        {
            this.topicName = topicName;
        }

        public bool hasPrevious()
        {
            return previous;
        }

        public void setPrevious(bool paired)
        {
            previous = paired;
        }

        public void addReply(string reply)
        {
            this.reply.Add(reply);
        }

        public string[] listReplies()
        {
            return reply.ToArray();
        }

        public void addRedirect(string meant)
        {
            _redirect.Add(meant);
        }

        public string[] listRedirects()
        {
            return _redirect.ToArray();
        }

        public void addCondition(string condition)
        {
            this.condition.Add(condition);
        }

        public string[] listConditions()
        {
            return condition.ToArray();
        }

        public string getPattern()
        {
            return this.pattern;
        }

        internal bool hasRedirect()
        {
            return this.redirect.Length > 0;
        }
    }
}
