using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// The passage between two 'worlds' (i.e. OverWorld and Cave).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(TwoSidedTile))]
    public class PassageController : Interactable
	{
        public delegate void EnterPassage(Vector2 position, PassageController passageController);
        public static event EnterPassage OnEnterPassage;

        public override bool HasBeenUsed
        {
            get { return base.HasBeenUsed; }

            set
            {
                base.HasBeenUsed = value;
                if (boxCollider == null) { boxCollider = GetComponent<BoxCollider2D>(); }
                boxCollider.enabled = !HasBeenUsed;
            }
        }

        // Componenets
        private TwoSidedTile twoSidedTileComponent;

        private void Awake()
        {
            // Get references to all components.
            twoSidedTileComponent = GetComponent<TwoSidedTile>();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (HasBeenUsed) { return; }

            if (collision.attachedRigidbody.tag == "Player")
            {
                HasBeenUsed = true;
                if (OnEnterPassage != null) { OnEnterPassage(transform.position, this); }
            }
        }

        /// <summary>
        /// Toggle the passage sprite to the opposite side.
        /// </summary>
        public void UpdateSprite()
        {
            if (HasBeenUsed)
            {
                twoSidedTileComponent.SetSpriteToBack();
            }
            else
            {
                twoSidedTileComponent.SetSpriteToFront();
            }
        }
    }
}
