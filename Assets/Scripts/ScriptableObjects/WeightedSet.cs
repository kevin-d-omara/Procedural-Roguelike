using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// List of prefab-weight pairs.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Data/Weighted Set", order = 8)]
	public class WeightedSet : ScriptableObject
	{
        public List<WeightedPairGO> list;
	}
}
