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
        [NonSerialized] public GameObject holder;
        [NonSerialized] public Dictionary<Vector3, GameObject> existing;
        [NonSerialized] public WeightedRandomSet<GameObject> randomizer;

        [SerializeField] private List<WeightedPairGO> obstacles = new List<WeightedPairGO>();

        public ObstacleInfo()
        {
            randomizer = new WeightedRandomSet<GameObject>();
            foreach (WeightedPairGO pair in obstacles)
            {
                randomizer.Add(pair.item, pair.weight);
            }
        }
    }
}
