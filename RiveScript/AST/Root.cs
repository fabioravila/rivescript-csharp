using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.AST
{
    public class Root
    {
        public Begin begin { get; set; } = new Begin();
        public IDictionary<string, Topic> topics { get; set; } = new Dictionary<string, Topic>();
        public List<ObjectMacro> objects { get; set; }


        public Topic getTopic(string name)
        {
            return topics[name];
        }

        public void addTopic(string name)
        {
            topics.Add(name, new Topic(name));
        }

        public void addObject(ObjectMacro obj)
        {
            objects.Add(obj);
        }
    }
}
