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
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            // Get a random two-sided tile from the set.
            spriteRenderer = GetComponent<SpriteRenderer>();
            twoSidedSprite = twoSidedTileSet.RandomTwoSidedSprite;
        }

        private void Start()
        {
            SetSpriteToFront();
        }

        public void SetSpriteToFront()
        {
            spriteRenderer.sprite = twoSidedSprite.GetFront();
        }

        public void SetSpriteToBack()
        {
            spriteRenderer.sprite = twoSidedSprite.GetBack();
        }
    }
}
