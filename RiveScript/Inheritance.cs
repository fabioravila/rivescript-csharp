using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    /// <summary>
    ///  An inheritance tracker to aid in sorting replies.
    /// </summary>
    public class Inheritance
    {
        private Dictionary<int, ICollection<string>> atomic = new Dictionary<int, ICollection<string>>(); //Whole words, no wildcards
        private Dictionary<int, ICollection<string>> option = new Dictionary<int, ICollection<string>>(); //Witch [optional] parts
        private Dictionary<int, ICollection<string>> alpha = new Dictionary<int, ICollection<string>>(); //With _alpha_ wildcards
        private Dictionary<int, ICollection<string>> number = new Dictionary<int, ICollection<string>>(); //With #number# wildcard
        private Dictionary<int, ICollection<string>> wild = new Dictionary<int, ICollection<string>>(); //With *start* wildcards
        private ICollection<string> pound = new List<string>(); //With only # in them
        private ICollection<string> under = new List<string>(); //With only _ in them
        private ICollection<string> star = new List<string>(); //With only * in them

        public Inheritance() { }

        /// <summary>
        /// Dumb the buckets out and add them to the given collection
        /// </summary>
        /// <param name="sorted"></param>
        /// <returns></returns>
        public ICollection<string> Dump(ICollection<string> sorted)
        {
            // Sort each sort-category by the number of words they have, in descending order.
            sorted = addSortedList(sorted, atomic);
            sorted = addSortedList(sorted, option);
            sorted = addSortedList(sorted, alpha);
            sorted = addSortedList(sorted, number);
            sorted = addSortedList(sorted, wild);

            // add the singleton wildcards too.
            sorted = addSortedList(sorted, under);
            sorted = addSortedList(sorted, pound);
            sorted = addSortedList(sorted, star);

            return sorted;
        }

        /// <summary>
        /// A helper function for sortReplies, adds a hash of (word count -> triggers vector) to the * running sort buffer.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        private ICollection<string> addSortedList(ICollection<string> vector, IDictionary<int, ICollection<string>> hash)
        {
            // We've been given a hash where the keys are integers (word counts) and
            // the values are all triggers with that number of words in them (where
            // words are things that aren't wildcards).

            //Sort the hash by its number of words, descendins
            var sortedKeys = Util.SortKeysDesc(hash);

            for (int i = 0; i < sortedKeys.Length; i++)
            {
                var itens = hash[sortedKeys[i]].ToArray();
                vector.AddRange(itens);
            }

            return vector;
        }

        /// <summary>
        /// A helper function for sortReplies, adds a vector of wildcard triggers to the running sort buffer.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="wc"></param>
        /// <returns></returns>
        private ICollection<string> addSortedList(ICollection<string> vector, ICollection<string> wc)
        {
            var itens = Util.SortByLengthDesc(wc.ToArray());
            vector.AddRange(itens);
            return vector;
        }

        public void addAtomic(int wc, string trigger)
        {
            if (false == atomic.ContainsKey(wc))
            {
                atomic.Add(wc, new List<string>());
            }

            atomic[wc].Add(trigger);
        }

        public void addOption(int wc, string trigger)
        {
            if (false == option.ContainsKey(wc))
            {
                option.Add(wc, new List<string>());
            }
            option[wc].Add(trigger);
        }

        public void addAlpha(int wc, string trigger)
        {
            if (false == alpha.ContainsKey(wc))
            {
                alpha.Add(wc, new List<string>());
            }
            alpha[wc].Add(trigger);
        }

        public void addNumber(int wc, string trigger)
        {
            if (false == number.ContainsKey(wc))
            {
                number.Add(wc, new List<string>());
            }
            number[wc].Add(trigger);
        }

        public void addWild(int wc, string trigger)
        {
            if (false == wild.ContainsKey(wc))
            {
                wild.Add(wc, new List<string>());
            }
            wild[wc].Add(trigger);
        }

        public void addPound(string trigger)
        {
            pound.Add(trigger);
        }

        public void addUnder(string trigger)
        {
            under.Add(trigger);
        }

        public void addStar(string trigger)
        {
            star.Add(trigger);
        }
    }
}
