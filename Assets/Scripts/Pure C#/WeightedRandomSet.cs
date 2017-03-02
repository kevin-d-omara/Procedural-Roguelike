using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class WeightedRandomSet<TKey>
    {
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Total weight of all items included in the set.
        /// </summary>
		public int TotalWeight { get; private set; }

        /// <summary>
        /// Contains the actual items.
        /// </summary>
        private Dictionary<TKey, int> items = new Dictionary<TKey, int>();

        public WeightedRandomSet()
        {
            TotalWeight = 0;
        }

        /// <summary>
        /// Selects a random item from the set taking into account each item's weight.
        /// </summary>
        public TKey RandomItem()
        {
            var weightCount = Random.Range(0, TotalWeight); // [0, TotalWeight) inclusive-exclusive
            foreach (KeyValuePair<TKey, int> item in items)
            {
                weightCount -= item.Value;
                if (weightCount < 0)
                {
                    return item.Key;
                }
            }
            return default(TKey);
        }

        /// <summary>
        /// Adds the item to the set.
        /// </summary>
        /// <param name="item">Item to add to the set.</param>
        /// <param name="weight">Weight of this item.</param>
        public void Add(TKey item, int weight)
        {
            items.Add(item, weight);
            TotalWeight += weight;
        }

        /// <summary>
        /// Gets the weight of the item specified.
        /// </summary>
        /// <param name="item">Item to check the weight of.</param>
        /// <returns>Weight of the item in question, or -1 if not found.</returns>
        public int GetWeight(TKey item)
        {
            int weight;
            if (items.TryGetValue(item, out weight))
            {
                return weight;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Updates the weight of the item specified.
        /// </summary>
        /// <param name="item">Item to update the weight of.</param>
        /// <param name="weight">Value to set the item's weight to.</param>
        public void SetWeight(TKey item, int weight)
        {
            if (items.ContainsKey(item))
            {
                items[item] = weight;
            }
        }
    }
}
