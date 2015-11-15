using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    /// <summary>
    /// Trigger class for RiveScript.
    /// </summary>
    public class Trigger
    {
        private string pattern = "";
        private string inTopic = "";
        private ICollection<string> redirect = new List<string>();
        private ICollection<string> reply = new List<string>();
        private ICollection<string> condition = new List<string>();
        private bool previous = false;


        public Trigger(string topic, string pattern)
        {
            inTopic = topic;
            this.pattern = pattern;
        }

        public string Topic
        {
            get
            {
                return inTopic;
            }
        }

        public bool HasPrevious
        {
            get
            {
                return previous;
            }
            set
            {
                //original implementation
                //previous = true;
                previous = value;
            }
        }

        public void AddReply(string reply)
        {
            this.reply.Add(reply);
        }

        public string[] Replies
        {
            get
            {
                return reply.ToArray();
            }
        }

        public void AddRedirect(string meant)
        {
            redirect.Add(meant);
        }

        public string[] Redirects
        {
            get
            {
                return redirect.ToArray();
            }
        }

        public void AddCondition(string condition)
        {
            this.condition.Add(condition);
        }

        public string[] Conditions
        {
            get
            {
                return condition.ToArray();
            }
        }
    }
}
