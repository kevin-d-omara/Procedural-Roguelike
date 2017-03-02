using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Container for floor prefab and pointers to all floor instances.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Data/Info (Floor)", order = 3)]
    public class FloorInfo : ScriptableObject
    {
        [NonSerialized] public Transform holder;
        [NonSerialized] public Dictionary<Vector3, GameObject> existing
            = new Dictionary<Vector3, GameObject>();
        public GameObject prefab;
    }
}
