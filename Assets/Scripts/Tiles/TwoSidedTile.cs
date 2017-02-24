using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Attach to a GameObject which has a single randomized two-sided tile.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TwoSidedTile : MonoBehaviour
	{
        [SerializeField] private TwoSidedTileSet twoSidedTileSet;
        private TwoSidedTileSet.TwoSidedSprite twoSidedSprite;

        private void Start()
        {
            // Get a random two-sided tile from the set.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            twoSidedSprite = twoSidedTileSet.RandomTwoSidedSprite;
            spriteRenderer.sprite = twoSidedSprite.GetFront();
        }
    }
}
