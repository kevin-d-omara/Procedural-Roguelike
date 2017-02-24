using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    [CreateAssetMenu(fileName = "Data", menuName = "Data/TileSet (2 sided)", order = 2)]
    public class TwoSidedTileSet : ScriptableObject
    {
        [Serializable]
        public class TwoSidedSprite
        {
            [SerializeField] private Sprite front;
            [SerializeField] private Sprite back;

            public Sprite GetFront() { return front; }
            public Sprite GetBack()  { return back; }
        }

        [SerializeField] private TwoSidedSprite[] sprites;

        public TwoSidedSprite RandomTwoSidedSprite
        {
            get { return sprites[Random.Range(0, sprites.Length)]; }
        }
    }
}
