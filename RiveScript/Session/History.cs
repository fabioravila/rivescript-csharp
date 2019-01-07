using System.Collections.Generic;

namespace RiveScript.Session
{
    public class History
    {
        public List<string> input { get; private set; }
        public List<string> reply { get; private set; }

        public History()
        {
            input = new List<string>();
            reply = new List<string>();

            for (int i = 0; i < Constants.HISTORY_SIZE; i++)
            {
                input.Add(Constants.Undefined);
                reply.Add(Constants.Undefined);
            }
        }
    }
}
