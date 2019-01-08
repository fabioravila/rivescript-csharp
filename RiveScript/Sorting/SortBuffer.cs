using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Sorting
{
    /// <summary>
    /// Sort buffer data.
    /// </summary>
    public class SortBuffer
    {
        public IDictionary<string, List<SortedTriggerEntry>> topics { get; private set; } = new Dictionary<string, List<SortedTriggerEntry>>();
        public IDictionary<string, List<SortedTriggerEntry>> thats { get; private set; } = new Dictionary<string, List<SortedTriggerEntry>>();
        public ICollection<string> sub { get; private set; } = new List<string>();
        public ICollection<string> person { get; private set; } = new List<string>();

        public SortBuffer()
        {

        }

        public void addTopic(string name, List<SortedTriggerEntry> triggers)
        {
            topics.AddOrUpdate(name, triggers);
        }

        public void addThats(string name, List<SortedTriggerEntry> triggers)
        {
            topics.AddOrUpdate(name, triggers);
        }

        public void setThats(List<string> sub)
        {
            this.sub = sub;
        }

        public void setPerson(List<string> person)
        {
            this.person = person;
        }
    }
}
