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

        public string topic()
        {
            return inTopic;
        }

        public bool hasPrevious()
        {
            return previous;
        }

        public void hasPrevious(bool paired)
        {
            //original implementation
            previous = true;
            //previous = paired; - My inserstant
        }

        public void addReply(string reply)
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

        public void addRedirect(string meant)
        {
            redirect.Add(meant);
        }

        public string[] listRedirects()
        {
            return redirect.ToArray();
        }

        public void addCondition(string condition)
        {
            this.condition.Add(condition);
        }

        public string[] listConditions()
        {
            return condition.ToArray();
        }
    }
}
