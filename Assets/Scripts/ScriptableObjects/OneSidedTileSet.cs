using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    [CreateAssetMenu(fileName = "Data", menuName = "Data/TileSet (1 sided)", order = 1)]
    public class OneSidedTileSet : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        public Sprite RandomSprite
        {
            get { return sprites[Random.Range(0, sprites.Length)]; }
        }
    }
}
