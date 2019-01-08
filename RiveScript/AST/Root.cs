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
        public TopicManager topicManager = new TopicManager();


        public Topic getTopic(string name)
        {
            return topicManager.topic(name);
        }

        public void addTopic(string name)
        {
            topicManager.topic(name);
        }

        public void addObject(ObjectMacro obj)
        {
            objects.Add(obj);
        }
    }
}
