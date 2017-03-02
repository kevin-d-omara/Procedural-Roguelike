using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Tuple for pairing data with weight.
    /// </summary>
    [Serializable]
	public class WeightedPair<T>
	{
        public T item;
        [Range(0, 20)]
        public int weight;
    }

    /// <summary>
    /// Tuple for pairing GameObject with weight.
    /// </summary>
    [Serializable]
    public class WeightedPairGO : WeightedPair<GameObject> { }
}
