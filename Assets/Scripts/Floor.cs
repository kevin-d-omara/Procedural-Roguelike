using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    [RequireComponent (typeof (SpriteRenderer))]
    public class Floor : MonoBehaviour
    {
        [SerializeField] private TileSet floorTiles;

        private void Start()
        {
            // Get a random floor tile sprite.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = floorTiles.RandomSprite;
        }
    }
}
