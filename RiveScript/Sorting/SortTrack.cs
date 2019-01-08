using System.Collections.Generic;

namespace RiveScript.Sorting
{
    /// <summary>
    /// Temporary categorization of triggers while sorting.
    /// </summary>
    public class SortTrack
    {
        public IDictionary<int, List<SortedTriggerEntry>> atomic { get; private set; } // Sort by number of whole words
        public IDictionary<int, List<SortedTriggerEntry>> option { get; private set; } // Sort optionals by number of words
        public IDictionary<int, List<SortedTriggerEntry>> alpha { get; private set; } // Sort alpha wildcards by no. of words
        public IDictionary<int, List<SortedTriggerEntry>> number { get; private set; } // Sort numeric wildcards by no. of words
        public IDictionary<int, List<SortedTriggerEntry>> wild { get; private set; } // Sort wildcards by no. of words
        public List<SortedTriggerEntry> pound { get; private set; } // Triggers of just '#'
        public List<SortedTriggerEntry> under { get; private set; } // Triggers of just '_'
        public List<SortedTriggerEntry> star { get; private set; } // Triggers of just '*'


        public SortTrack()
        {
            atomic = new Dictionary<int, List<SortedTriggerEntry>>();
            option = new Dictionary<int, List<SortedTriggerEntry>>();
            alpha = new Dictionary<int, List<SortedTriggerEntry>>();
            number = new Dictionary<int, List<SortedTriggerEntry>>();
            wild = new Dictionary<int, List<SortedTriggerEntry>>();
            pound = new List<SortedTriggerEntry>();
            under = new List<SortedTriggerEntry>();
            star = new List<SortedTriggerEntry>();
        }
    }
}
