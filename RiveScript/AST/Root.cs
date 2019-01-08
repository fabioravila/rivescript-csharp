using System;
using System.Collections.Generic;

namespace RiveScript.AST
{
    /// <summary>
    /// Represents the root of the Abstract Syntax Tree (AST).
    /// </summary>
    public class Root
    {
        public Begin begin { get; set; } = new Begin();
        public List<ObjectMacro> objects { get; set; } = new List<ObjectMacro>();
        public IDictionary<string, Topic> topics = new Dictionary<string, Topic>();

        public void addTopic(Topic topic)
        {
            topics.AddOrUpdate(topic.name, topic);
        }

        public void addObject(ObjectMacro obj)
        {
            objects.Add(obj);
        }
    }
}
