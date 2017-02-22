using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [CreateAssetMenu(fileName = "Data", menuName = "Tiles/Floor", order = 1)]
    public class FloorTiles : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        public Sprite[] Sprites()
        {
            return sprites;
        }
    }
}
