using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Container for prefab-weight pairs and pointers to all obstacle instances. Can be used to
    /// describe a themed or balanced set of obstacles (i.e. Rock heavy, etc.).
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Data/Info (Obstacles)", order = 4)]
	public class ObstacleInfo : ScriptableObject
	{
        [NonSerialized] public Transform holder;
        public WeightedRandomSet<GameObject> Randomizer
        {
            get
            {
                if (_randomizer.Count == 0)
                {
                    foreach (WeightedPairGO pair in obstacles)
                    {
                        _randomizer.Add(pair.item, pair.weight);
                    }
                }
                return _randomizer;
            }
        }
        private WeightedRandomSet<GameObject> _randomizer = new WeightedRandomSet<GameObject>();

        [SerializeField] private List<WeightedPairGO> obstacles = new List<WeightedPairGO>();
    }
}
