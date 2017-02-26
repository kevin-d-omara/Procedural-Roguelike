using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Attach to a GameObject which has a single randomized one-sided tile.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class OneSidedTile : MonoBehaviour
	{
        [SerializeField] private OneSidedTileSet oneSidedTileSet;

        private void Start()
        {
            // Get a random one-sided tile from the set.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = oneSidedTileSet.RandomSprite;
        }
    }
}
