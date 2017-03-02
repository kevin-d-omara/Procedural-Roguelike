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
        [NonSerialized] public Transform holder;
        [NonSerialized] public List<GameObject> existing = new List<GameObject>();

        public WeightedRandomSet<GameObject> Randomizer
        {
            get
            {
                if (_randomizer.Count == 0)
                {
                    foreach (WeightedPairGO pair in enemies)
                    {
                        _randomizer.Add(pair.item, pair.weight);
                    }
                }
                return _randomizer;
            }
        }
        private WeightedRandomSet<GameObject> _randomizer = new WeightedRandomSet<GameObject>();

        [SerializeField] private List<WeightedPairGO> enemies = new List<WeightedPairGO>();
    }
}
