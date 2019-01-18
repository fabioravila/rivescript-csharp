namespace RiveScript.Sorting
{
    /// <summary>
    /// Holds a sorted trigger and the pointer to that trigger's data
    /// </summary>
    public class SortedTriggerEntry
    {
        public string trigger { get; set; }
        public string pointer { get; set; }

        public SortedTriggerEntry(string trigger, string pointer)
        {
            this.trigger = trigger;
            this.pointer = pointer;
        }
    }
}