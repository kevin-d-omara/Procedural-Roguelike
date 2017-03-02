using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Container for prefab-weight pairs and pointers to all enemy instances. Can be used to
    /// describe a themed or balanced set of enemies (i.e. Mostly bats, zombies, etc.).
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Data/Info (Enemies)", order = 5)]
    public class EnemyInfo : ScriptableObject
    {
        [NonSerialized] public GameObject holder;
        [NonSerialized] public List<GameObject> existing;
        [NonSerialized] public WeightedRandomSet<GameObject> randomizer;

        [SerializeField] private List<WeightedPairGO> enemies = new List<WeightedPairGO>();

        public EnemyInfo()
        {
            randomizer = new WeightedRandomSet<GameObject>();
            foreach (WeightedPairGO pair in enemies)
            {
                randomizer.Add(pair.item, pair.weight);
            }
        }
    }
}
