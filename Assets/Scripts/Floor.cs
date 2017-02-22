using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    [RequireComponent (typeof (SpriteRenderer))]
    public class Floor : MonoBehaviour
    {
        [SerializeField] private FloorTiles floorTiles;

        private void Start()
        {
            // randomly select a floor sprite from FloorTiles data
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Sprite[] sprites = floorTiles.Sprites();
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        }
    }
}
